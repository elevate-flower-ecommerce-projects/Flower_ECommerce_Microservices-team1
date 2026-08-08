namespace Identity_service.Entities;

/// <summary>Security-relevant events for the admin back-office.</summary>
public class AdminSecurityAudit
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Path { get; set; }
}
