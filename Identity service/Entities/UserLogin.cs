namespace Identity_service.Entities;

public class UserLogin
{
    public string LoginProvider { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? ProviderDisplayName { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}
