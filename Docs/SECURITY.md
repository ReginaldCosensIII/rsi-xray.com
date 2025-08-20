# Security Policy

## Supported Versions
This project uses **ASP.NET Core 8 Razor Pages**. Security patches and dependency updates will be applied to:

- `main` (production-ready branch)
- Active feature branches under development

## Reporting a Vulnerability
If you discover a vulnerability:

1. **Do not** open a public GitHub issue.
2. Email the project maintainer directly (contact: `security@rsi-xray.com`).
3. Provide as much detail as possible:
   - Steps to reproduce
   - Potential impact
   - Suggested remediation (if known)

You can expect an acknowledgment within **48 hours** and a remediation plan within **7 days**.

## Security Measures Implemented
- **Form Protection**
  - Google reCAPTCHA v2 integration
  - ASP.NET Core built-in **rate limiting** middleware
- **Email Handling**
  - Safe headers, sanitized subjects
  - No PII in logs
  - Branded HTML templates stored in `/Email/Templates`
- **Configuration Security**
  - SMTP credentials & API keys stored in **User Secrets** (Dev) and **Environment Variables** (Prod)
  - No hardcoded secrets in code
- **Network Security**
  - HTTPS enforced
  - TLS required for SMTP delivery
- **Error Handling**
  - Visitor-safe error messages
  - Internal logging via `ILogger`
- **Dependency Management**
  - NuGet dependencies monitored and patched regularly
- **Robots & Sitemap**
  - `/robots.txt` and `/sitemap.xml` added to control indexing

## Future Security Enhancements
- Replace fallback plain-text emails with fully branded HTML-only flows
- Optional migration to OAuth2 SMTP (for providers that support it)
- Centralized audit logging and monitoring
