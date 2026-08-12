using System.Text.Json;
using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Persistence;

public interface ICatalogDataSeeder
{
    Task SeedAsync();
}

public sealed class CatalogDataSeeder(CatalogDbContext dbContext) : ICatalogDataSeeder
{
    private static readonly Guid BirthdayCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid RosesCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid PlantsCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid WeddingOccasionId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BirthdayOccasionId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid BannerId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    public async Task SeedAsync()
    {
        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { Id = BirthdayCategoryId, Name = "Birthday Flowers", ImageUrl = "/images/categories/birthday.jpg", SortOrder = 1 },
                new Category { Id = RosesCategoryId, Name = "Roses", ImageUrl = "/images/categories/roses.jpg", SortOrder = 2 },
                new Category { Id = PlantsCategoryId, Name = "Plants", ImageUrl = "/images/categories/plants.jpg", SortOrder = 3 });
        }

        if (!await dbContext.Occasions.AnyAsync())
        {
            dbContext.Occasions.AddRange(
                new Occasion { Id = WeddingOccasionId, Name = "Wedding", ImageUrl = "/images/occasions/wedding.jpg", SortOrder = 1 },
                new Occasion { Id = BirthdayOccasionId, Name = "Birthday", ImageUrl = "/images/occasions/birthday.jpg", SortOrder = 2 });
        }

        await SeedProductsAsync();

        if (!await dbContext.Banners.AnyAsync())
        {
            dbContext.Banners.Add(new Banner
            {
                Id = BannerId,
                ImageUrl = "/images/banners/home-hero.jpg",
                DeepLink = "/products?collection=summer",
                SortOrder = 1
            });
        }

        if (!await dbContext.HomeSections.AnyAsync())
        {
            dbContext.HomeSections.AddRange(
                new HomeSection
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    Type = "banner",
                    Title = "Summer Picks",
                    Order = 1,
                    Enabled = true,
                    ContentRefJson = JsonSerializer.Serialize(new { bannerId = BannerId })
                },
                new HomeSection
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                    Type = "category_rail",
                    Title = "Shop by Category",
                    Order = 2,
                    Enabled = true,
                    ContentRefJson = JsonSerializer.Serialize(new { take = 10, deepLink = "/categories" })
                },
                new HomeSection
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                    Type = "product_rail",
                    Title = "Best Sellers",
                    Order = 3,
                    Enabled = true,
                    ContentRefJson = JsonSerializer.Serialize(new { selectionRule = "best_sellers", take = 10, deepLink = "/products?sort=best_sellers" })
                },
                new HomeSection
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                    Type = "occasion_rail",
                    Title = "Occasions",
                    Order = 4,
                    Enabled = true,
                    ContentRefJson = JsonSerializer.Serialize(new { take = 10, deepLink = "/occasions" })
                });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedProductsAsync()
    {
        var products = ProductSeedData.Create(
            BirthdayCategoryId,
            RosesCategoryId,
            PlantsCategoryId,
            WeddingOccasionId,
            BirthdayOccasionId);

        var productIds = products.Select(product => product.Id).ToArray();
        var existingProductIds = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .Select(product => product.Id)
            .ToListAsync();

        var missingProducts = products
            .Where(product => !existingProductIds.Contains(product.Id))
            .ToArray();

        if (missingProducts.Length > 0)
            dbContext.Products.AddRange(missingProducts);
    }
}
