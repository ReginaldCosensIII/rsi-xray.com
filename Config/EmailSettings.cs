namespace RSIWebsiteBackend.Config
{
    public class EmailSettings
    {
        public string? Mode { get; set; }              // "Pickup" or "Smtp" (future)
        public string? From { get; set; }
        public string? To { get; set; }
        public string? PickupDirectory { get; set; }   // for Pickup mode

        // (optional SMTP fields for later)
        public string? Host { get; set; }
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
