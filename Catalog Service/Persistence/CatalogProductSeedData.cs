using Catalog_Service.Entities;

namespace Catalog_Service.Persistence;

internal static class ProductSeedData
{
    private static readonly Guid NasrCityStoreId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid MaadiStoreId = Guid.Parse("60000000-0000-0000-0000-000000000002");
    private static readonly HashSet<int> UnavailableProductNumbers = [5, 18, 29, 40];

    public static IReadOnlyList<Product> Create(
        Guid birthdayCategoryId,
        Guid rosesCategoryId,
        Guid plantsCategoryId,
        Guid weddingOccasionId,
        Guid birthdayOccasionId)
    {
        var utcNow = DateTime.UtcNow;

        return
        [
            Product("001", "Classic Red Roses", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=800&q=80", 499, rosesCategoryId, birthdayOccasionId, 180, discountPercent: 15, activeFrom: utcNow.AddDays(-7), activeTo: utcNow.AddDays(21)),
            Product("002", "Sunrise Birthday Bouquet", "https://images.unsplash.com/photo-1561181286-d3fee7d55364?auto=format&fit=crop&w=800&q=80", 650, birthdayCategoryId, birthdayOccasionId, 132),
            Product("003", "Peace Lily Plant", "https://images.unsplash.com/photo-1593482892290-f54927ae1bf6?auto=format&fit=crop&w=800&q=80", 720, plantsCategoryId, null, 96, discountPercent: 20, activeFrom: utcNow.AddDays(-30), activeTo: utcNow.AddDays(-1)),
            Product("004", "Blush Wedding Bouquet", "https://images.unsplash.com/photo-1519225429780-3f5309403066?auto=format&fit=crop&w=800&q=80", 950, rosesCategoryId, weddingOccasionId, 77, discountPercent: 10, activeFrom: utcNow.AddDays(3), activeTo: utcNow.AddDays(20)),
            Product("005", "Gardenia Gift Box", "https://images.unsplash.com/photo-1526047932273-341f2a7631f9?auto=format&fit=crop&w=800&q=80", 840, birthdayCategoryId, birthdayOccasionId, 44),
            Product("006", "Maadi Orchid Arrangement", "https://images.unsplash.com/photo-1525310072745-f49212b5ac6d?auto=format&fit=crop&w=800&q=80", 1100, plantsCategoryId, weddingOccasionId, 63, discountPercent: 12.5m, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(14)),
            Product("007", "Pastel Peony Basket", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 780, birthdayCategoryId, birthdayOccasionId, 51, discountPercent: 25, activeFrom: utcNow.AddDays(-21), activeTo: utcNow.AddDays(-2)),
            Product("008", "Amber Rose Bouquet", "https://images.unsplash.com/photo-1563241527-3004b7be0ffd?auto=format&fit=crop&w=800&q=80", 560, rosesCategoryId, birthdayOccasionId, 118),
            Product("009", "Apricot Tulip Bundle", "https://images.unsplash.com/photo-1518895949257-7621c3c786d7?auto=format&fit=crop&w=800&q=80", 610, birthdayCategoryId, birthdayOccasionId, 87, discountPercent: 10, activeFrom: utcNow.AddDays(-1), activeTo: utcNow.AddDays(10)),
            Product("010", "Baby Breath Cloud", "https://images.unsplash.com/photo-1533616688419-b7a58556458e?auto=format&fit=crop&w=800&q=80", 430, rosesCategoryId, weddingOccasionId, 39),
            Product("011", "Blooming Anthurium", "https://images.unsplash.com/photo-1614594975525-e45190c55d0b?auto=format&fit=crop&w=800&q=80", 890, plantsCategoryId, null, 72),
            Product("012", "Blue Hydrangea Box", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 760, birthdayCategoryId, birthdayOccasionId, 91, discountPercent: 18, activeFrom: utcNow.AddDays(-4), activeTo: utcNow.AddDays(16)),
            Product("013", "Bridal White Roses", "https://images.unsplash.com/photo-1533616688419-b7a58556458e?auto=format&fit=crop&w=800&q=80", 1200, rosesCategoryId, weddingOccasionId, 105),
            Product("014", "Candlelight Carnations", "https://images.unsplash.com/photo-1582794543139-8ac9cb0f7b11?auto=format&fit=crop&w=800&q=80", 470, birthdayCategoryId, birthdayOccasionId, 48),
            Product("015", "Champagne Rose Vase", "https://images.unsplash.com/photo-1559563458-527698bf5295?auto=format&fit=crop&w=800&q=80", 990, rosesCategoryId, weddingOccasionId, 66, discountPercent: 8, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(8)),
            Product("016", "Cherry Blossom Branches", "https://images.unsplash.com/photo-1522383225653-ed111181a951?auto=format&fit=crop&w=800&q=80", 680, birthdayCategoryId, birthdayOccasionId, 43),
            Product("017", "Coral Peony Bouquet", "https://images.unsplash.com/photo-1563241527-3004b7be0ffd?auto=format&fit=crop&w=800&q=80", 860, rosesCategoryId, birthdayOccasionId, 83),
            Product("018", "Desert Rose Plant", "https://images.unsplash.com/photo-1509423350716-97f9360b4e09?auto=format&fit=crop&w=800&q=80", 540, plantsCategoryId, null, 29),
            Product("019", "Elegant Eucalyptus Vase", "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?auto=format&fit=crop&w=800&q=80", 690, plantsCategoryId, weddingOccasionId, 57),
            Product("020", "Forever Pink Roses", "https://images.unsplash.com/photo-1561181286-d3fee7d55364?auto=format&fit=crop&w=800&q=80", 1040, rosesCategoryId, birthdayOccasionId, 124, discountPercent: 20, activeFrom: utcNow.AddDays(-5), activeTo: utcNow.AddDays(25)),
            Product("021", "Golden Sunflower Basket", "https://images.unsplash.com/photo-1597848212624-a19eb35e2651?auto=format&fit=crop&w=800&q=80", 630, birthdayCategoryId, birthdayOccasionId, 74),
            Product("022", "Graceful Lily Bouquet", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 820, rosesCategoryId, weddingOccasionId, 68),
            Product("023", "Hanging Ivy Plant", "https://images.unsplash.com/photo-1485955900006-10f4d324d411?auto=format&fit=crop&w=800&q=80", 460, plantsCategoryId, null, 35),
            Product("024", "Ivory Wedding Centerpiece", "https://images.unsplash.com/photo-1519225429780-3f5309403066?auto=format&fit=crop&w=800&q=80", 1450, rosesCategoryId, weddingOccasionId, 112, discountPercent: 15, activeFrom: utcNow.AddDays(-10), activeTo: utcNow.AddDays(18)),
            Product("025", "Jasmine Garden Basket", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 730, plantsCategoryId, birthdayOccasionId, 46),
            Product("026", "Lavender Love Bouquet", "https://images.unsplash.com/photo-1520763185298-1b434c919102?auto=format&fit=crop&w=800&q=80", 700, birthdayCategoryId, birthdayOccasionId, 89, discountPercent: 5, activeFrom: utcNow.AddDays(-1), activeTo: utcNow.AddDays(5)),
            Product("027", "Lemon Lime Dracaena", "https://images.unsplash.com/photo-1485955900006-10f4d324d411?auto=format&fit=crop&w=800&q=80", 590, plantsCategoryId, null, 31),
            Product("028", "Midnight Blue Iris", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 775, rosesCategoryId, weddingOccasionId, 58, discountPercent: 30, activeFrom: utcNow.AddDays(2), activeTo: utcNow.AddDays(20)),
            Product("029", "Mint Garden Planter", "https://images.unsplash.com/photo-1593482892290-f54927ae1bf6?auto=format&fit=crop&w=800&q=80", 510, plantsCategoryId, birthdayOccasionId, 41),
            Product("030", "Moonlight Orchid Duo", "https://images.unsplash.com/photo-1525310072745-f49212b5ac6d?auto=format&fit=crop&w=800&q=80", 1320, plantsCategoryId, weddingOccasionId, 97, discountPercent: 12, activeFrom: utcNow.AddDays(-3), activeTo: utcNow.AddDays(12)),
            Product("031", "Nile Lotus Arrangement", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 920, birthdayCategoryId, birthdayOccasionId, 62),
            Product("032", "Olive Tree Gift", "https://images.unsplash.com/photo-1509423350716-97f9360b4e09?auto=format&fit=crop&w=800&q=80", 1150, plantsCategoryId, weddingOccasionId, 54),
            Product("033", "Peach Gerbera Basket", "https://images.unsplash.com/photo-1563241527-3004b7be0ffd?auto=format&fit=crop&w=800&q=80", 580, birthdayCategoryId, birthdayOccasionId, 70, discountPercent: 10, activeFrom: utcNow.AddDays(-12), activeTo: utcNow.AddDays(-3)),
            Product("034", "Pearl Rose Heart", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=800&q=80", 980, rosesCategoryId, weddingOccasionId, 103),
            Product("035", "Pink Tulip Melody", "https://images.unsplash.com/photo-1520763185298-1b434c919102?auto=format&fit=crop&w=800&q=80", 640, birthdayCategoryId, birthdayOccasionId, 79),
            Product("036", "Royal Purple Orchid", "https://images.unsplash.com/photo-1525310072745-f49212b5ac6d?auto=format&fit=crop&w=800&q=80", 1280, plantsCategoryId, weddingOccasionId, 85, discountPercent: 17.5m, activeFrom: utcNow.AddDays(-6), activeTo: utcNow.AddDays(19)),
            Product("037", "Ruby Red Bouquet", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=800&q=80", 720, rosesCategoryId, birthdayOccasionId, 110),
            Product("038", "Silver Leaf Plant", "https://images.unsplash.com/photo-1485955900006-10f4d324d411?auto=format&fit=crop&w=800&q=80", 490, plantsCategoryId, null, 37),
            Product("039", "Summer Meadow Bouquet", "https://images.unsplash.com/photo-1508610048659-a06b669e3321?auto=format&fit=crop&w=800&q=80", 760, birthdayCategoryId, birthdayOccasionId, 93, discountPercent: 22, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(15)),
            Product("040", "White Lily Wedding Vase", "https://images.unsplash.com/photo-1533616688419-b7a58556458e?auto=format&fit=crop&w=800&q=80", 1180, rosesCategoryId, weddingOccasionId, 101)
        ];
    }

    private static Product Product(
        string idSuffix,
        string name,
        string imageUrl,
        decimal price,
        Guid categoryId,
        Guid? occasionId,
        int soldCount,
        decimal? discountPercent = null,
        DateTime? activeFrom = null,
        DateTime? activeTo = null)
        => new()
        {
            Id = Guid.Parse($"40000000-0000-0000-0000-000000000{idSuffix}"),
            Name = name,
            ImageUrl = imageUrl,
            Price = price,
            DiscountPercent = discountPercent,
            DiscountStartsAtUtc = activeFrom,
            DiscountEndsAtUtc = activeTo,
            CategoryId = categoryId,
            OccasionId = occasionId,
            SoldCount = soldCount,
            Description = $"A carefully arranged {name} prepared with fresh flowers and gift-ready presentation."
        };

    public static IReadOnlyList<ProductImage> CreateImages(IReadOnlyList<Product> products)
        => products.SelectMany(product =>
        {
            var number = GetProductNumber(product.Id);
            var cover = product.ImageUrl;
            var detail = product.ImageUrl;

            return new[]
            {
                new ProductImage
                {
                    Id = SeedId("70000000", number * 10 + 1),
                    ProductId = product.Id,
                    ImageUrl = cover,
                    SortOrder = 1
                },
                new ProductImage
                {
                    Id = SeedId("70000000", number * 10 + 2),
                    ProductId = product.Id,
                    ImageUrl = detail,
                    SortOrder = 2
                }
            };
        }).ToArray();

    public static IReadOnlyList<ProductIncludedItem> CreateIncludedItems(IReadOnlyList<Product> products)
        => products.SelectMany(product =>
        {
            var number = GetProductNumber(product.Id);

            return new[]
            {
                new ProductIncludedItem
                {
                    Id = SeedId("80000000", number * 10 + 1),
                    ProductId = product.Id,
                    Name = "Fresh seasonal flowers",
                    Quantity = 1,
                    SortOrder = 1
                },
                new ProductIncludedItem
                {
                    Id = SeedId("80000000", number * 10 + 2),
                    ProductId = product.Id,
                    Name = "Gift wrapping",
                    Quantity = 1,
                    SortOrder = 2
                }
            };
        }).ToArray();

    public static IReadOnlyList<ProductStoreInventory> CreateStoreInventories(IReadOnlyList<Product> products)
        => products.SelectMany(product =>
        {
            var number = GetProductNumber(product.Id);
            return new[]
            {
                Inventory(number, product, NasrCityStoreId, !UnavailableProductNumbers.Contains(number) && number % 9 != 0),
                Inventory(number, product, MaadiStoreId, !UnavailableProductNumbers.Contains(number) && number % 7 != 0)
            };
        }).ToArray();

    private static ProductStoreInventory Inventory(int productNumber, Product product, Guid storeId, bool isEnabled)
        => new()
        {
            Id = SeedId("90000000", productNumber * 10 + (storeId == NasrCityStoreId ? 1 : 2)),
            ProductId = product.Id,
            StoreId = storeId,
            AvailableQuantity = isEnabled ? 10 : 0,
            IsEnabled = isEnabled
        };

    private static int GetProductNumber(Guid productId)
        => int.Parse(productId.ToString("N")[^12..]);

    private static Guid SeedId(string prefix, int sequence)
        => Guid.Parse($"{prefix}-0000-0000-0000-{sequence:D12}");
}
