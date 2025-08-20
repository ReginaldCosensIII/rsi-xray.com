// -----------------------------------------------------------------------------
// File: Services/EmailService.cs
// Author: RSI Website Rebuild (ASP.NET Core 8 Razor Pages)
// Description:
//   Centralized email sender for the Contact form. Uses SmtpSettings from
//   configuration and composes both (1) internal notification and (2) visitor
//   confirmation using branded HTML templates + plain-text alternative.
//   This version constructs proper multipart/alternative by adding both
//   AlternateViews (text/plain first, then text/html) and does NOT set Body.
// Security/Hardening: unchanged
// Notes:
//   Ensure templates are copied to output via .csproj:
//     <ItemGroup>
//       <Content Include="Email\Templates\**\*.html">
//         <CopyToOutputDirectory>Always</CopyToOutputDirectory>
//       </Content>
//     </ItemGroup>
// -----------------------------------------------------------------------------

using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using RSIWebsiteBackend.Config;

// alias the existing form type to avoid long names
using ContactForm = RSIWebsiteBackend.Pages.ContactUsModel.ContactForm;

namespace RSIWebsiteBackend.Services
{
    public interface IEmailService
    {
        Task SendInternalAsync(ContactForm data, CancellationToken ct = default);
        Task SendVisitorConfirmationAsync(ContactForm data, CancellationToken ct = default);
    }

    public sealed class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly IHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpOptions,
                            IHostEnvironment env,
                            ILogger<EmailService> logger)
        {
            _smtp = smtpOptions.Value;
            _env = env;
            _logger = logger;
        }

        // --------------------- PUBLIC API ---------------------

        public async Task SendInternalAsync(ContactForm data, CancellationToken ct = default)
        {
            var subjectPrefix = _env.IsDevelopment() ? "[DEV] " : "";
            var safeSubject = SanitizeSubject(data.Subject);
            var subject = $"{subjectPrefix}[RSI Contact] {safeSubject}";

            // Load + merge branded HTML template
            var htmlBody = LoadAndMergeHtml("Email/Templates/InternalNotification.html", data);

            // Plain text fallback for deliverability
            var textBody = BuildPlainTextInternal(data);

            using var msg = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_smtp.FromEmail) ? "no-reply@rsi-xray.com" : _smtp.FromEmail,
                    string.IsNullOrWhiteSpace(_smtp.FromName) ? "RSI Website" : _smtp.FromName
                ),
                Subject = subject,
                // do NOT set Body / IsBodyHtml, we use AlternateViews instead
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            // Decide internal recipient (dev routes to DefaultTo)
            var to = ResolveInternalTo(data);
            msg.To.Add(to);

            // Replies go to the visitor’s email
            if (!string.IsNullOrWhiteSpace(data.Email))
                msg.ReplyToList.Add(new MailAddress(data.Email, data.Name));

            // Proper multipart/alternative: text first, then HTML
            var altText = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
            var altHtml = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
            msg.AlternateViews.Add(altText);
            msg.AlternateViews.Add(altHtml);

            await SendAsync(msg, ct);
        }

        public async Task SendVisitorConfirmationAsync(ContactForm data, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(data.Email)) return;
            if (string.IsNullOrWhiteSpace(_smtp.FromEmail) || string.IsNullOrWhiteSpace(_smtp.Host))
                throw new InvalidOperationException("SMTP not configured for confirmation email.");

            var subjectPrefix = _env.IsDevelopment() ? "[DEV] " : "";
            var subject = $"{subjectPrefix}We’ve received your message — RSI";

            var htmlBody = LoadAndMergeHtml("Email/Templates/VisitorConfirmation.html", data);
            var textBody = BuildPlainTextVisitor(data);

            using var msg = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_smtp.FromEmail) ? "no-reply@rsi-xray.com" : _smtp.FromEmail,
                    string.IsNullOrWhiteSpace(_smtp.FromName) ? "RSI Website" : _smtp.FromName
                ),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            msg.To.Add(new MailAddress(data.Email, data.Name));

            // In dev, optionally BCC internal to see confirmations too
            if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(_smtp.DefaultTo))
                msg.Bcc.Add(_smtp.DefaultTo);

            // Proper multipart/alternative: text first, then HTML
            var altText = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
            var altHtml = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
            msg.AlternateViews.Add(altText);
            msg.AlternateViews.Add(altHtml);

            await SendAsync(msg, ct);
        }

        // --------------------- SMTP SEND ---------------------

        private async Task SendAsync(MailMessage msg, CancellationToken ct)
        {
            using var smtp = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = _smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                Timeout = 1000 * 30 // 30s
            };

            _logger.LogInformation("[SMTP SEND] host={Host}:{Port} ssl={Ssl}", _smtp.Host, _smtp.Port, _smtp.EnableSsl);
            await smtp.SendMailAsync(msg, ct);
        }

        // --------------------- TEMPLATING ---------------------

        private static string LoadAndMergeHtml(string relativePath, ContactForm data)
        {
            var html = LoadFile(relativePath);

            // Merge tokens (HTML-escaped where appropriate)
            html = html.Replace("{{Name}}", WebUtility.HtmlEncode(data.Name))
                       .Replace("{{Email}}", WebUtility.HtmlEncode(data.Email))
                       .Replace("{{Phone}}", WebUtility.HtmlEncode(data.Phone))
                       .Replace("{{Subject}}", WebUtility.HtmlEncode(data.Subject))
                       // Preserve user newlines as <br>, but HTML-escape first
                       .Replace("{{Message}}", ToHtmlWithBreaks(data.Message))
                       .Replace("{{SiteName}}", "RSI")
                       .Replace("{{Year}}", DateTime.UtcNow.Year.ToString())
                       .Replace("{{SubmittedAt}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
                       .Replace("{{FromEmail}}", "no-reply@rsi-xray.com");

            return html;
        }

        private static string LoadFile(string relativePath)
        {
            var baseDir = AppContext.BaseDirectory;
            var full = Path.Combine(baseDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
                throw new FileNotFoundException($"Email template not found: {full}");
            return File.ReadAllText(full, Encoding.UTF8);
        }

        private static string ToHtmlWithBreaks(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var safe = WebUtility.HtmlEncode(value);
            return safe.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br>");
        }

        // --------------------- HELPERS ---------------------

        private static string SanitizeSubject(string? input, int maxLen = 120)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Website Contact";
            var s = input.Replace("\r", " ").Replace("\n", " ").Trim();
            return s.Length > maxLen ? s[..maxLen] : s;
        }

        private string ResolveInternalTo(ContactForm data)
        {
            if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(_smtp.DefaultTo))
                return _smtp.DefaultTo;

            // PROD routing: use configured default; as a last resort, fall back to FromEmail
            return !string.IsNullOrWhiteSpace(_smtp.DefaultTo) ? _smtp.DefaultTo : _smtp.FromEmail;
        }

        private static string BuildPlainTextInternal(ContactForm data)
        {
            var sb = new StringBuilder()
                .AppendLine("A new contact form was submitted on the RSI website.")
                .AppendLine()
                .AppendLine($"Name: {data.Name}")
                .AppendLine($"Email: {data.Email}")
                .AppendLine($"Phone: {data.Phone}")
                .AppendLine($"Subject: {data.Subject}")
                .AppendLine()
                .AppendLine("Message:")
                .AppendLine(data.Message);
            return sb.ToString();
        }

        private static string BuildPlainTextVisitor(ContactForm data)
        {
            var sb = new StringBuilder()
                .AppendLine($"Hello {data.Name},")
                .AppendLine()
                .AppendLine("Thanks for contacting RSI. We’ve received your message and someone will be in touch shortly.")
                .AppendLine()
                .AppendLine($"Subject: {data.Subject}")
                .AppendLine($"Name: {data.Name}")
                .AppendLine($"Email: {data.Email}")
                .AppendLine($"Phone: {data.Phone}")
                .AppendLine()
                .AppendLine("Message:")
                .AppendLine(data.Message)
                .AppendLine()
                .AppendLine("— RSI Team");
            return sb.ToString();
        }
    }
}
