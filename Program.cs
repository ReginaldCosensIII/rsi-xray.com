// -----------------------------------------------------------------------------
// File: Program.cs
// Author: RSI Website Rebuild (ASP.NET Core 8 Razor Pages)
// Description:
//   App entry point. Configures services (Razor Pages, options binding/validation,
//   HttpClient, response compression, rate limiting) and the HTTP pipeline (HTTPS/HSTS,
//   security headers, status code pages, static files, routing).
// Notes:
//   - CSRF protection is applied globally via MVC filter
//     (AutoValidateAntiforgeryTokenAttribute).
//   - Tune the Content-Security-Policy (CSP) hosts to only what you load.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;               // AutoValidateAntiforgeryTokenAttribute
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RSIWebsiteBackend.Config;               // RecaptchaSettings, SmtpSettings
using System.Threading.RateLimiting;          // rate limiting
using Microsoft.AspNetCore.RateLimiting;

namespace RSIWebsiteBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- Strongly-typed options + validation (fail fast on missing config) ----
            builder.Services.AddOptions<RecaptchaSettings>()
                .Bind(builder.Configuration.GetSection("Recaptcha"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.SiteKey), "Recaptcha:SiteKey is required.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "Recaptcha:SecretKey is required.")
                .ValidateOnStart();

            builder.Services.AddOptions<SmtpSettings>()
                .Bind(builder.Configuration.GetSection("SmtpSettings"))
                .Validate(s => !string.IsNullOrWhiteSpace(s.Host), "SmtpSettings:Host is required.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "SmtpSettings:Username is required.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "SmtpSettings:Password is required.")
                .ValidateOnStart();

            // HttpClient for reCAPTCHA verification calls, etc.
            builder.Services.AddHttpClient();

            // Enable gzip/brotli for text/css/js/json
            builder.Services.AddResponseCompression();

            // Razor Pages
            builder.Services.AddRazorPages();

            // Dependency injection for services
            builder.Services.AddScoped<RSIWebsiteBackend.Services.IEmailService, RSIWebsiteBackend.Services.EmailService>();

            // ✅ Global CSRF protection for unsafe HTTP methods (MVC-level filter)
            builder.Services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            // ---- Rate Limiting: throttle contact form posts ----
            builder.Services.AddRateLimiter(options =>
            {
                // At most 5 POSTs per minute per IP (tweak as needed)
                options.AddFixedWindowLimiter("contact-posts", o =>
                {
                    o.PermitLimit = 5;
                    o.Window = TimeSpan.FromMinutes(1);
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    o.QueueLimit = 0; // reject instead of queue
                });
            });

            var app = builder.Build();

            // ---- Middleware pipeline ----
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // === SECURITY HEADERS MIDDLEWARE ===
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
                // Content Security Policy: adjust hosts as needed (recaptcha, fonts, maps, CDNs).
                ctx.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "img-src 'self' data: https://maps.googleapis.com https://maps.gstatic.com; " +
                    "script-src 'self' 'unsafe-inline' https://www.google.com/recaptcha/ https://www.gstatic.com/recaptcha/; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                    "frame-src https://www.google.com/;";
                await next();
            });
            // === END SECURITY HEADERS MIDDLEWARE ===

            // Enable gzip/brotli
            app.UseResponseCompression();

            // Rate limiter (must be early in the pipeline)
            app.UseRateLimiter();

            // Friendly error pages for 404/500 with re-execute pattern
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            // Static files (consider adding long Cache-Control if using hashed filenames)
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
