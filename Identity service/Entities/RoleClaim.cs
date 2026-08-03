namespace Identity_service.Entities;

public class RoleClaim
{
    public int Id { get; set; }

    public Guid RoleId { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public Role Role { get; set; } = null!;
}
