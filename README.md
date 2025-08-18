# RSI Website Backend

ASP.NET Core 8 backend project for **Radiometric Services & Instruments (RSI)** website rebuild.  
This backend provides secure form handling, email delivery, templating, and SEO support for a modern, responsive frontend.

---

## 🚀 Features

- **Razor Pages Templating**  
  Shared layouts for consistent header, hero, and footer sections.

- **Secure Contact Form**  
  Google reCAPTCHA v2 validation and SMTP-based email delivery.

- **Email Notifications**  
  Sends form submissions to RSI staff and confirmation emails to users.

- **SEO Optimization**  
  Per-page meta data, structured schema markup, and semantic HTML.

- **Extensible Architecture**  
  Designed to scale with additional pages and integrations.

---

## 📂 Project Structure

RSIWebsiteBackend/
│
├── Config/ # Strongly typed config classes (SMTP, reCAPTCHA)
├── Pages/ # Razor Pages (e.g., Index.cshtml, ContactUs.cshtml)
├── Services/ # Email service and helpers
├── wwwroot/ # Static assets (CSS, JS, images)
├── appsettings.json # Main configuration file
├── Program.cs # Application entry point
└── README.md # Project documentation

---

## 🛡️ Security Considerations

- All form submissions are validated with **Google reCAPTCHA v2**.  
- No SMTP credentials or API keys are stored in the repo.  
- Only **development settings** are tracked in version control (`appsettings.Development.json`).  
- Production secrets should be injected via environment variables or server-level configuration.  

---

## 🔧 Development Workflow

1. Clone the repository:
   git clone https://github.com/<YourUsername>/RSIWebsiteBackend.git
   cd RSIWebsiteBackend
Restore dependencies:
dotnet restore
Add your local development secrets to appsettings.Development.json:
{
  "RecaptchaSettings": {
    "SiteKey": "your-dev-site-key",
    "SecretKey": "your-dev-secret-key"
  },
  "SmtpSettings": {
    "Host": "smtp.devprovider.com",
    "Port": 587,
    "Username": "dev@example.com",
    "Password": "yourpassword"
  }
}

Run the project locally:
dotnet run

📦 Deployment
Development: Hosted locally with .Development.json configs.

Production:

Configure SMTP with RSI domain email.

Replace reCAPTCHA keys with RSI credentials.

Publish using dotnet publish and deploy to IIS.

🧩 Future Enhancements
Branded HTML email templates

Logging with Serilog or NLog

Centralized error handling middleware

Continuous Integration (CI) pipeline with GitHub Actions

📄 License
Proprietary – Internal use only by Radiometric Services & Instruments (RSI).

👨‍💻 Maintainer
Developed by Reggie Cosens
LinkedIn • GitHub

---
