namespace Identity_service.Domain.Entities;

public class UserToken
{
    public Guid UserId { get; set; }

    public string LoginProvider { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public User User { get; set; } = null!;
}
