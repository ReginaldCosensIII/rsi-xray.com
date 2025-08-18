// -----------------------------------------------------------------------------
// File: ContactUs.cshtml.cs
// Author: RSI Website Rebuild (ASP.NET Core 8 Razor Pages)
// Description:
//   PageModel for the "Contact Us" form. Handles model binding, validation,
//   Google reCAPTCHA verification (server-side), and delegates email sending
//   to Services.EmailService. Implements CSRF (class-level), honeypot check,
//   rate limiting, and minimal logging.
// Notes:
//   - On success, PRG to ContactThanks.cshtml. On errors, return Page().
//   - Exposes RecaptchaSiteKey only if you choose to use it in the view;
//     current view injects IOptions<RecaptchaSettings> directly.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RSIWebsiteBackend.Config;
using RSIWebsiteBackend.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RSIWebsiteBackend.Pages
{
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact-posts")]
    public class ContactUsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RecaptchaSettings _recaptcha;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ContactUsModel> _logger;
        private readonly IEmailService _email;

        public ContactUsModel(
            IHttpClientFactory httpClientFactory,
            IOptions<RecaptchaSettings> recaptchaOptions,
            ILogger<ContactUsModel> logger,
            IHostEnvironment env,
            IEmailService email)
        {
            _httpClientFactory = httpClientFactory;
            _recaptcha = recaptchaOptions.Value;
            _logger = logger;
            _env = env;
            _email = email;
        }

        // If you prefer to bind in the view, you can also expose this:
        public string RecaptchaSiteKey => _recaptcha.SiteKey ?? string.Empty;

        [BindProperty]
        public ContactForm FormData { get; set; } = new ContactForm();

        public bool FormSubmitted { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            var q = Request.Query["subject"].ToString();
            if (!string.IsNullOrWhiteSpace(q))
            {
                FormData.Subject = q;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the highlighted fields and try again.";
                return Page();
            }

            // Normalize phone (very simple)
            FormData.Phone = NormalizePhone(FormData.Phone);
            if (string.IsNullOrWhiteSpace(FormData.Phone))
            {
                ModelState.AddModelError("FormData.Phone", "Please enter a valid phone number.");
                ErrorMessage = "Please correct the highlighted fields and try again.";
                return Page();
            }

            // Honeypot
            if (!string.IsNullOrEmpty(Request.Form["website"]))
            {
                ErrorMessage = "Spam detected. If this is an error, please contact us by phone.";
                return Page();
            }

            // reCAPTCHA verify
            var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
            var recaptchaOk = await VerifyRecaptchaV2Async(recaptchaToken);
            if (!recaptchaOk)
            {
                ErrorMessage = "reCAPTCHA verification failed. Please try again.";
                return Page();
            }

            try
            {
                // 1) Internal notification
                await _email.SendInternalAsync(FormData);

                // 2) Visitor confirmation
                await _email.SendVisitorConfirmationAsync(FormData);

                FormSubmitted = true;
                ModelState.Clear();
                FormData = new ContactForm();

                return RedirectToPage("ContactThanks");
            }
            catch (SmtpException smtpEx)
            {
                ErrorMessage = "There was an error sending your message. Please try again later.";
                _logger.LogWarning(smtpEx, "[SMTP ERROR] while sending contact form");
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "There was an error sending your message. Please try again later.";
                _logger.LogWarning(ex, "[ERROR] while sending contact form");
                return Page();
            }
        }

        // ------- reCAPTCHA v2 verify -------
        private async Task<bool> VerifyRecaptchaV2Async(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_recaptcha.SecretKey))
                return false;

            var client = _httpClientFactory.CreateClient();

            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _recaptcha.SecretKey),
                new KeyValuePair<string, string>("response", token),
                new KeyValuePair<string, string>("remoteip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "")
            });

            var resp = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            if (!resp.IsSuccessStatusCode) return false;

            var json = await resp.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RecaptchaV2Result>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result is null) return false;
            return result.success;
        }

        // Simple phone normalization: keep digits and one leading +
        private static string NormalizePhone(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            raw = raw.Trim();

            var sb = new StringBuilder();
            bool plusUsed = false;

            foreach (var ch in raw)
            {
                if (char.IsDigit(ch)) sb.Append(ch);
                else if (ch == '+' && !plusUsed && sb.Length == 0) { sb.Append('+'); plusUsed = true; }
            }

            var normalized = sb.ToString();
            int digitCount = normalized.Count(char.IsDigit);
            if (digitCount < 10) return "";
            return normalized;
        }

        // Form DTO (kept here to avoid wider refactors right now)
        public class ContactForm
        {
            [Required, StringLength(200)]
            public string Name { get; set; } = "";

            [Required, EmailAddress, StringLength(300)]
            public string Email { get; set; } = "";

            [Required, StringLength(30)]
            [Phone(ErrorMessage = "Please enter a valid phone number.")]
            public string Phone { get; set; } = "";

            [Required, StringLength(200)]
            public string Subject { get; set; } = "General Inquiry";

            [Required, StringLength(5000)]
            public string Message { get; set; } = "";
        }

        private sealed class RecaptchaV2Result
        {
            public bool success { get; set; }
            public string? challenge_ts { get; set; }
            public string? hostname { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? error_codes { get; set; }
        }
    }
}
