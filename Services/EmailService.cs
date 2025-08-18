// -----------------------------------------------------------------------------
// File: Services/EmailService.cs
// Author: RSI Website Rebuild (ASP.NET Core 8 Razor Pages)
// Description:
//   Centralized email sender for the Contact form. Uses SmtpSettings from
//   configuration and composes both (1) internal notification and (2) visitor
//   confirmation. Safe headers, minimal logging (no PII in logs), and explicit
//   fallbacks. Instantiates SmtpClient per-send (it is not thread-safe).
// Notes:
//   - Expects SmtpSettings to be supplied via User Secrets (Dev) or Env Vars (Prod).
//   - Works with the existing RSIWebsiteBackend.Pages.ContactUsModel.ContactForm
//     type to avoid ripples; if you later extract a shared DTO, it's one method signature to change.
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

        public async Task SendInternalAsync(ContactForm data, CancellationToken ct = default)
        {
            var subjectPrefix = _env.IsDevelopment() ? "[DEV] " : "";
            var safeSubject = SanitizeSubject(data.Subject);

            // Plain-text body (internal)
            var body = new StringBuilder()
                .AppendLine("A new contact form was submitted on the RSI website.")
                .AppendLine()
                .AppendLine($"Name: {data.Name}")
                .AppendLine($"Email: {data.Email}")
                .AppendLine($"Phone: {data.Phone}")
                .AppendLine($"Subject: {data.Subject}")
                .AppendLine()
                .AppendLine("Message:")
                .AppendLine(data.Message)
                .ToString();

            using var msg = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_smtp.FromEmail) ? "no-reply@rsi-dev.com" : _smtp.FromEmail,
                    string.IsNullOrWhiteSpace(_smtp.FromName) ? "RSI Website" : _smtp.FromName
                ),
                Subject = $"{subjectPrefix}[RSI Contact] {safeSubject}",
                Body = body,
                IsBodyHtml = false
            };

            // Decide internal recipient
            string? to = null;
            if (_env.IsDevelopment())
            {
                if (!string.IsNullOrWhiteSpace(_smtp.DefaultTo))
                    to = _smtp.DefaultTo;
            }
            else
            {
                to = !string.IsNullOrWhiteSpace(_smtp.DefaultTo) ? _smtp.DefaultTo : data.Email;
            }

            if (string.IsNullOrWhiteSpace(to))
                to = "test@example.com"; // final fallback

            msg.To.Add(to);

            // Replies go to the visitor’s email
            if (!string.IsNullOrWhiteSpace(data.Email))
                msg.ReplyToList.Add(new MailAddress(data.Email));

            await SendAsync(msg, ct);
        }

        public async Task SendVisitorConfirmationAsync(ContactForm data, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(data.Email)) return;
            if (string.IsNullOrWhiteSpace(_smtp.FromEmail) || string.IsNullOrWhiteSpace(_smtp.Host))
                throw new InvalidOperationException("SMTP not configured for confirmation email.");

            var subjectPrefix = _env.IsDevelopment() ? "[DEV] " : "";
            var subject = $"{subjectPrefix}We’ve received your message — RSI";

            var html = BuildVisitorConfirmationHtml(data);
            var textBody = BuildVisitorConfirmationText(data);

            using var msg = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_smtp.FromEmail) ? "no-reply@rsi-dev.com" : _smtp.FromEmail,
                    string.IsNullOrWhiteSpace(_smtp.FromName) ? "RSI Website" : _smtp.FromName
                ),
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };

            msg.To.Add(new MailAddress(data.Email, data.Name));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));

            await SendAsync(msg, ct);
        }

        // ---- SMTP send (single place) ----
        private async Task SendAsync(MailMessage msg, CancellationToken ct)
        {
            using var smtp = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = _smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                Timeout = 1000 * 30 // 30s safety
            };

            _logger.LogInformation("[SMTP SEND] host={Host}:{Port} ssl={Ssl}", _smtp.Host, _smtp.Port, _smtp.EnableSsl);
            await smtp.SendMailAsync(msg, ct);
        }

        // ---- Helpers ----
        private static string SanitizeSubject(string? input, int maxLen = 120)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Website Contact";
            var s = input.Replace("\r", " ").Replace("\n", " ").Trim();
            return s.Length > maxLen ? s[..maxLen] : s;
        }

        private static string BuildVisitorConfirmationHtml(ContactForm data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.AppendLine("<title>Thanks for contacting RSI</title></head><body style=\"font-family:Arial,Helvetica,sans-serif;line-height:1.5;color:#222;\">");
            sb.AppendLine($"<h2 style=\"margin:0 0 16px 0;\">Hello {System.Net.WebUtility.HtmlEncode(data.Name)},</h2>");
            sb.AppendLine("<p style=\"margin:0 0 16px 0;\">Thanks for reaching out to RSI. We’ve received your message and someone from our team will be in touch shortly.</p>");
            sb.AppendLine("<p style=\"margin:0 0 16px 0;\">Here’s a copy of what you sent:</p>");
            sb.AppendLine("<hr style=\"border:none;border-top:1px solid #ddd;margin:16px 0;\" />");
            sb.AppendLine("<div>");
            sb.AppendLine($"<p style=\"margin:0 0 8px 0;\"><strong>Subject:</strong> {System.Net.WebUtility.HtmlEncode(data.Subject)}</p>");
            sb.AppendLine($"<p style=\"margin:0 0 8px 0;\"><strong>Name:</strong> {System.Net.WebUtility.HtmlEncode(data.Name)}</p>");
            sb.AppendLine($"<p style=\"margin:0 0 8px 0;\"><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(data.Email)}</p>");
            sb.AppendLine($"<p style=\"margin:0 0 8px 0;\"><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(data.Phone)}</p>");
            sb.AppendLine("<p style=\"margin:0 0 8px 0;\"><strong>Message:</strong></p>");
            sb.AppendLine($"<p style=\"white-space:pre-wrap;margin:0 0 8px 0;\">{System.Net.WebUtility.HtmlEncode(data.Message)}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<hr style=\"border:none;border-top:1px solid #ddd;margin:16px 0;\" />");
            sb.AppendLine("<p style=\"margin:0;\">— RSI Team</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string BuildVisitorConfirmationText(ContactForm data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Hello {data.Name},");
            sb.AppendLine();
            sb.AppendLine("Thanks for reaching out to RSI. We’ve received your message and someone from our team will be in touch shortly.");
            sb.AppendLine();
            sb.AppendLine("Subject: " + data.Subject);
            sb.AppendLine("Name: " + data.Name);
            sb.AppendLine("Email: " + data.Email);
            sb.AppendLine("Phone: " + data.Phone);
            sb.AppendLine();
            sb.AppendLine("Message:");
            sb.AppendLine(data.Message);
            sb.AppendLine();
            sb.AppendLine("— RSI Team");
            return sb.ToString();
        }
    }
}
