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
                new Category { Id = BirthdayCategoryId, Name = "Bouquets", ImageUrl = "categories/bouquets.png", SortOrder = 1 },
                new Category { Id = RosesCategoryId, Name = "Roses", ImageUrl = "categories/roses.png", SortOrder = 2 },
                new Category { Id = PlantsCategoryId, Name = "Accessories", ImageUrl = "categories/damond.png", SortOrder = 3 });
        }

        if (!await dbContext.Occasions.AnyAsync())
        {
            dbContext.Occasions.AddRange(
                new Occasion { Id = WeddingOccasionId, Name = "Wedding", ImageUrl = "https://images.unsplash.com/photo-1519225429780-3f5309403066?auto=format&fit=crop&w=800&q=80", SortOrder = 1 },
                new Occasion { Id = BirthdayOccasionId, Name = "Birthday", ImageUrl = "https://images.unsplash.com/photo-1558636508-e0db3814bd1d?auto=format&fit=crop&w=800&q=80", SortOrder = 2 });
        }

        if (!await dbContext.Products.AnyAsync())
        {
            var products = ProductSeedData.Create(
                BirthdayCategoryId,
                RosesCategoryId,
                PlantsCategoryId,
                WeddingOccasionId,
                BirthdayOccasionId);

            dbContext.Products.AddRange(products);
            dbContext.ProductImages.AddRange(ProductSeedData.CreateImages(products));
            dbContext.ProductIncludedItems.AddRange(ProductSeedData.CreateIncludedItems(products));
            dbContext.ProductStoreInventories.AddRange(ProductSeedData.CreateStoreInventories(products));
        }

        if (!await dbContext.Banners.AnyAsync())
        {
            dbContext.Banners.Add(new Banner
            {
                Id = BannerId,
                ImageUrl = "https://images.unsplash.com/photo-1526047932273-341f2a7631f9?auto=format&fit=crop&w=1200&q=80",
                DeepLink = "/products?collection=summer",
                SortOrder = 1
            });
        }

        // Update any existing entities that still have placeholder /images/ URLs
        var existingCategories = await dbContext.Categories.ToListAsync();
        foreach (var cat in existingCategories)
        {
            if (string.IsNullOrEmpty(cat.ImageUrl) || cat.ImageUrl.StartsWith("/images/"))
            {
                cat.ImageUrl = cat.Name switch
                {
                    "Birthday Flowers" or "Bouquets" => "categories/bouquets.png",
                    "Roses" => "categories/roses.png",
                    "Plants" or "Accessories" => "categories/damond.png",
                    "Tulips" => "categories/tulips.png",
                    "Gifts" => "categories/gift.png",
                    "Cards" => "categories/card.png",
                    _ => "categories/tulip_flower.png"
                };
            }
        }

        var existingOccasions = await dbContext.Occasions.ToListAsync();
        foreach (var occ in existingOccasions)
        {
            if (string.IsNullOrEmpty(occ.ImageUrl) || occ.ImageUrl.StartsWith("/images/"))
            {
                occ.ImageUrl = occ.Name switch
                {
                    "Birthday" => "https://images.unsplash.com/photo-1558636508-e0db3814bd1d?auto=format&fit=crop&w=800&q=80",
                    "Anniversary" => "https://images.unsplash.com/photo-1515934751635-c81c6bc9a2d8?auto=format&fit=crop&w=800&q=80",
                    "Valentine's Day" or "Valentine" => "https://images.unsplash.com/photo-1518199266791-5375a83190b7?auto=format&fit=crop&w=800&q=80",
                    "Wedding" => "https://images.unsplash.com/photo-1519225429780-3f5309403066?auto=format&fit=crop&w=800&q=80",
                    _ => "https://images.unsplash.com/photo-1558636508-e0db3814bd1d?auto=format&fit=crop&w=800&q=80"
                };
            }
        }

        var existingProducts = await dbContext.Products.ToListAsync();
        foreach (var prod in existingProducts)
        {
            if (string.IsNullOrEmpty(prod.ImageUrl) || prod.ImageUrl.StartsWith("/images/"))
            {
                prod.ImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=800&q=80";
            }
        }

        var existingProductImages = await dbContext.ProductImages.ToListAsync();
        foreach (var img in existingProductImages)
        {
            if (string.IsNullOrEmpty(img.ImageUrl) || img.ImageUrl.StartsWith("/images/"))
            {
                img.ImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=800&q=80";
            }
        }

        var existingBanners = await dbContext.Banners.ToListAsync();
        foreach (var b in existingBanners)
        {
            if (string.IsNullOrEmpty(b.ImageUrl) || b.ImageUrl.StartsWith("/images/"))
            {
                b.ImageUrl = "https://images.unsplash.com/photo-1526047932273-341f2a7631f9?auto=format&fit=crop&w=1200&q=80";
            }
        }

        // Replace or seed HomeSections to match Team 3 structure
        var existingSections = await dbContext.HomeSections.ToListAsync();
        if (existingSections.Any())
        {
            dbContext.HomeSections.RemoveRange(existingSections);
            await dbContext.SaveChangesAsync();
        }

        dbContext.HomeSections.AddRange(
            new HomeSection
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Type = "Categories",
                Title = "Categories",
                TitleAr = "الفئات",
                Order = 0,
                Enabled = true,
                OccasionId = null,
                CategoryId = null,
                ContentRefJson = JsonSerializer.Serialize(new { titleAr = "الفئات", occasionId = (Guid?)null, categoryId = (Guid?)null })
            },
            new HomeSection
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Type = "BestSeller",
                Title = "Best seller",
                TitleAr = "الأكثر مبيعاً",
                Order = 1,
                Enabled = true,
                OccasionId = null,
                CategoryId = null,
                ContentRefJson = JsonSerializer.Serialize(new { titleAr = "الأكثر مبيعاً", occasionId = (Guid?)null, categoryId = (Guid?)null })
            },
            new HomeSection
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                Type = "Occasions",
                Title = "Occasion",
                TitleAr = "المناسبات",
                Order = 2,
                Enabled = true,
                OccasionId = null,
                CategoryId = null,
                ContentRefJson = JsonSerializer.Serialize(new { titleAr = "المناسبات", occasionId = (Guid?)null, categoryId = (Guid?)null })
            },
            new HomeSection
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                Type = "ProductsCarousel",
                Title = "Valentine's picks",
                TitleAr = "اختيارات عيد الحب",
                Order = 3,
                Enabled = true,
                OccasionId = WeddingOccasionId,
                CategoryId = null,
                ContentRefJson = JsonSerializer.Serialize(new { titleAr = "اختيارات عيد الحب", occasionId = WeddingOccasionId, categoryId = (Guid?)null })
            });

        await dbContext.SaveChangesAsync();
    }

}
