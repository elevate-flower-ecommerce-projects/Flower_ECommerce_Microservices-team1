namespace Identity_service.Settings;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Flower E-Commerce";
    public int MaxRetries { get; set; } = 2;
    public int InitialRetryDelaySeconds { get; set; } = 1;
}
