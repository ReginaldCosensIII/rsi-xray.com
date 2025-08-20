# Architecture Overview — RSI Website Rebuild

## Technology Stack
- **Frontend**: Razor Pages with HTML, CSS, JavaScript
- **Backend**: ASP.NET Core 8
- **Email**: System.Net.Mail with centralized `EmailService`
- **Security**: Google reCAPTCHA, ASP.NET Core Rate Limiter
- **Deployment**: IIS (Production), Dev Server (Staging)
- **Configuration**: JSON + User Secrets (Dev), Environment Variables (Prod)

## Key Components

### Razor Pages
- Structured into `/Pages`
- Shared layout: `_Layout.cshtml` with global header, footer, and hero sections
- Cohesive design across all pages

### Email Service
- Located in `/Services/EmailService.cs`
- Interfaces:
  - `SendInternalAsync` → sends notification to RSI staff
  - `SendVisitorConfirmationAsync` → sends branded confirmation to visitor
- Templates:
  - `/Email/Templates/InternalNotification.html`
  - `/Email/Templates/VisitorConfirmation.html`
- Security:
  - Safe headers
  - Sanitized subject lines
  - Fallbacks for null values

### Security & Validation
- **Rate Limiting**: Limits form submission attempts per IP
- **CAPTCHA**: Google reCAPTCHA v2 integrated on Contact page
- **Sanitization**: HTML-encode user input before rendering in emails
- **Logging**: Minimal logs, no PII

### SEO & Indexing
- `robots.txt` and `sitemap.xml` implemented
- Semantic HTML structure in templates
- Optimized metadata

## Deployment Workflow
1. Development → Local (Visual Studio 2022, Debug Mode)
2. Staging → Dev Server (HTTPS configured)
3. Production → IIS on Windows Server (with URL Rewrite planned for SEO-friendly slugs)

## Future Considerations
- Add centralized logging & monitoring
- OAuth2 SMTP integration
- Razor Class Library for shared UI components
- API endpoints for future form integrations
