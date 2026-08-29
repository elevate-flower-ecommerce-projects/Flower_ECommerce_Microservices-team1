namespace Address___Store_Coverage_Service.Entities;

public sealed class City
{
    public Guid Id { get; set; }
    public Guid AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system:seed";
    public DateTime? DeletedAt { get; set; }
}
