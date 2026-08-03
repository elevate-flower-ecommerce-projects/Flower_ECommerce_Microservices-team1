namespace Identity_service.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<RoleClaim> RoleClaims { get; set; } = [];
}
