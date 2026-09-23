namespace MaleFashion.Domain.Utilities

{
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public decimal MinimumScore { get; set; } = 0.5m;
        public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
        public bool IsEnabled { get; set; } = true;
    }
}

