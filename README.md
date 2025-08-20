RSI Website Rebuild – Backend (ASP.NET Core 8)
📌 Project Overview

This repository contains the secure, modern backend implementation for the RSI (Radiometric Services & Instruments, LLC) website rebuild project, replacing a legacy Classic ASP infrastructure. The backend is built using ASP.NET Core 8 Razor Pages and designed to support secure form submissions, branded email notifications, reCAPTCHA validation, and modular page templating.

The backend is tightly integrated with a responsive frontend and emphasizes security, SEO, performance, and lead generation.

🔐 Production readiness: Nearing completion. Pending SMTP finalization, branded emails, and final deployment settings.

📁 Project Structure
RSIWebsiteBackend/
├── Pages/                 # Razor Pages (.cshtml and .cshtml.cs)
│   ├── Index.cshtml       # Homepage
│   └── ContactUs.cshtml   # Contact form page
│
├── Services/
│   └── EmailService.cs    # IEmailService implementation for sending notifications
│
├── Templates/
│   ├── InternalNotification.html    # HTML email to RSI staff
│   └── VisitorConfirmation.html     # Confirmation email to user
│
├── Config/
│   ├── SmtpSettings.cs
│   └── RecaptchaSettings.cs
│
├── wwwroot/              # Static assets (CSS, JS, images)
├── appsettings.json      # Configuration (non-secret)
├── Program.cs            # Middleware, rate limiting, config binding
├── SECURITY.md           # Security audit summary & hardening steps
├── STATUS.md             # Deployment readiness checklist
├── ARCHITECTURE.md       # High-level design diagram + flow
└── README.md             # You're here
⚙️ Core Features

✅ Razor Pages templating engine with shared layout (_Layout.cshtml)

✅ Responsive SEO-ready pages (meta title, description, canonical)

✅ Secure Contact Form:

Field validation + CSRF protection

Google reCAPTCHA v2 integration

Honeypot anti-bot input

ASP.NET built-in rate limiting

✅ Email Notifications (via SMTP)

Internal notification to RSI team

Confirmation email to user

Branded HTML + plain-text fallback

✅ Environment-based secrets/config:

SMTP credentials via User Secrets (dev) or Env Vars (prod)

Separate settings for dev, staging, and production

🔐 Security Measures Implemented
Feature	Status
Rate Limiting	✅ Enabled
CSRF Protection	✅ Via Razor
reCAPTCHA v2	✅ Verified
SMTP Secrets Isolation	✅ Secured
CSP + HSTS (non-dev)	✅ Configured
Input Validation	✅ Annotations
Email Fallback Logic	✅ In place

See SECURITY.md for full audit notes and ongoing tasks.

🚀 Deployment Instructions

⚠️ This is a backend-only project. Frontend HTML/CSS/JS assets are authored separately and merged at publish time.

Development

dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet run

Navigate to: https://localhost:5001

Publishing for IIS

dotnet publish -c Release -o ./publish

Copy ./publish to IIS-bound directory

Ensure app pool user has read/execute permissions

Configure site binding for domain (e.g., test.rsi-xray.com)

Production

Store SMTP & Recaptcha keys in Environment Variables

Setup TLS (HTTPS) with trusted certificate

Update DNS, SPF/DKIM/DMARC for SMTP domain

Add CSP allowlist for Google domains

✏️ Branded Email Templates

/Templates/InternalNotification.html – sent to RSI staff

/Templates/VisitorConfirmation.html – sent to visitor

Temporary templates are live; final branding & customization are pending stakeholder feedback.

📄 Documentation

SECURITY.md – hardening overview

STATUS.md – feature checklist

ARCHITECTURE.md – project flow & layering

📍 Project Goals Recap

Migrate legacy site to secure, maintainable ASP.NET Core architecture

Build modular and extensible backend for lead capture, contact, future CTAs

Provide a foundation for future integrations (newsletter, CRM, downloads)

Ensure performance, security, and SEO compliance

👤 Contact & Ownership

Developer: Reggie Cosens
Hired by: CES Inc. on behalf of RSI
Client: Radiometric Services & Instruments, LLC
Project Status: Dev/QA complete – awaiting production SMTP & final content

For project handoff, issues, or staging access, please contact CES project coordinator or RSI technical contact.