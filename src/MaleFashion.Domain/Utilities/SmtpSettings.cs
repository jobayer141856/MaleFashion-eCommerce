namespace MaleFashion.Domain.Utilities
{

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string FromName { get; set; } = "Male Fashion";
        public string FromEmail { get; set; } = string.Empty;
        public int Timeout { get; set; } = 10000;
        public SmtpEncryptionTypes SmtpEncryption { get; set; } = SmtpEncryptionTypes.StartTls;
    }
}

