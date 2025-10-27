using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RSIWebsiteBackend.Config;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace RSIWebsiteBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- Strongly-typed options + validation ----
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

            builder.Services.AddHttpClient();
            builder.Services.AddResponseCompression();
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<RSIWebsiteBackend.Services.IEmailService, RSIWebsiteBackend.Services.EmailService>();

            // Global CSRF on unsafe HTTP verbs
            builder.Services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            // Rate limiting for contact form
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("contact-posts", o =>
                {
                    o.PermitLimit = 5;
                    o.Window = TimeSpan.FromMinutes(1);
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    o.QueueLimit = 0;
                });
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Security headers (adjust CSP hosts to your needs)
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";

                ctx.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "base-uri 'self'; " +
                    "object-src 'none'; " +
                    "form-action 'self'; " +
                    "frame-ancestors 'self'; " +
                    "upgrade-insecure-requests; " +
                    "img-src 'self' data: https://maps.googleapis.com https://maps.gstatic.com; " +
                    "script-src 'self' 'unsafe-inline' https://www.google.com/recaptcha/ https://www.gstatic.com/recaptcha/; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                    "frame-src https://www.google.com/;";

                await next();
            });

            app.UseResponseCompression();
            app.UseRateLimiter();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            // *** CONSOLIDATED STATIC FILES CONFIGURATION ***
            // This single call will serve static files AND apply caching headers.
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                }
            });

            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }
    }
}