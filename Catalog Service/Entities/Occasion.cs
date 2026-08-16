namespace Catalog_Service.Entities;

public sealed class Occasion
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ProductOccasion> ProductOccasions { get; set; } = [];
}
