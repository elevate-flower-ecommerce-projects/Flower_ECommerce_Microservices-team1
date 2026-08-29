namespace Address___Store_Coverage_Service.Entities;

public sealed class Area
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system:seed";
    public DateTime? DeletedAt { get; set; }

    public ICollection<City> Cities { get; set; } = new List<City>();
}
