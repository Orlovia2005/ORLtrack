namespace plt.Services.Ai
{
    public class GigaChatOptions
    {
        public bool Enabled { get; set; }
        public string AuthorizationKey { get; set; } = string.Empty;
        public string Scope { get; set; } = "GIGACHAT_API_PERS";
        public string Model { get; set; } = "GigaChat-2-Pro";
        public string OAuthUrl { get; set; } = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
        public string ChatUrl { get; set; } = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";
        public bool IgnoreSslCertificateErrors { get; set; }
        public int MaxStudentsPerRequest { get; set; } = 3;
    }
}
