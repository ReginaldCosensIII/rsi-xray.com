namespace RSIWebsiteBackend.Config
{
    // Strongly-typed settings bound from appsettings*.json: "Recaptcha": { ... }
    public class RecaptchaSettings
    {
        // Defaults avoid nullable warnings when the class is constructed.
        public string SiteKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        // If you ever switch to reCAPTCHA v3, you can store threshold here.
        public double ScoreThreshold { get; set; } = 0.5;
    }
}
