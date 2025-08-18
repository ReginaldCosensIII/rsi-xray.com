namespace RSIWebsiteBackend.Config
{
    // Strongly-typed settings bound from appsettings*.json: "SmtpSettings": { ... }
    public class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "RSI Website";
        public bool EnableSsl { get; set; } = true;
        // Optional default recipient for dev testing
        public string DefaultTo { get; set; } = "";
    }
}
