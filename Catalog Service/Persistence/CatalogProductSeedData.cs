using Catalog_Service.Entities;

namespace Catalog_Service.Persistence;

internal static class ProductSeedData
{
    private static readonly Guid NasrCityStoreId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid MaadiStoreId = Guid.Parse("60000000-0000-0000-0000-000000000002");

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
            Product("001", "Classic Red Roses", "red-roses", 499, rosesCategoryId, birthdayOccasionId, 180, discountPercent: 15, activeFrom: utcNow.AddDays(-7), activeTo: utcNow.AddDays(21)),
            Product("002", "Sunrise Birthday Bouquet", "sunrise-bouquet", 650, birthdayCategoryId, birthdayOccasionId, 132),
            Product("003", "Peace Lily Plant", "peace-lily", 720, plantsCategoryId, null, 96, discountPercent: 20, activeFrom: utcNow.AddDays(-30), activeTo: utcNow.AddDays(-1)),
            Product("004", "Blush Wedding Bouquet", "blush-wedding-bouquet", 950, rosesCategoryId, weddingOccasionId, 77, storeId: NasrCityStoreId, discountPercent: 10, activeFrom: utcNow.AddDays(3), activeTo: utcNow.AddDays(20)),
            Product("005", "Gardenia Gift Box", "gardenia-gift-box", 840, birthdayCategoryId, birthdayOccasionId, 44, isAvailable: false),
            Product("006", "Maadi Orchid Arrangement", "maadi-orchid-arrangement", 1100, plantsCategoryId, weddingOccasionId, 63, storeId: MaadiStoreId, discountPercent: 12.5m, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(14)),
            Product("007", "Pastel Peony Basket", "pastel-peony-basket", 780, birthdayCategoryId, birthdayOccasionId, 51, discountPercent: 25, activeFrom: utcNow.AddDays(-21), activeTo: utcNow.AddDays(-2)),
            Product("008", "Amber Rose Bouquet", "amber-rose-bouquet", 560, rosesCategoryId, birthdayOccasionId, 118),
            Product("009", "Apricot Tulip Bundle", "apricot-tulip-bundle", 610, birthdayCategoryId, birthdayOccasionId, 87, discountPercent: 10, activeFrom: utcNow.AddDays(-1), activeTo: utcNow.AddDays(10)),
            Product("010", "Baby Breath Cloud", "baby-breath-cloud", 430, rosesCategoryId, weddingOccasionId, 39),
            Product("011", "Blooming Anthurium", "blooming-anthurium", 890, plantsCategoryId, null, 72, storeId: NasrCityStoreId),
            Product("012", "Blue Hydrangea Box", "blue-hydrangea-box", 760, birthdayCategoryId, birthdayOccasionId, 91, discountPercent: 18, activeFrom: utcNow.AddDays(-4), activeTo: utcNow.AddDays(16)),
            Product("013", "Bridal White Roses", "bridal-white-roses", 1200, rosesCategoryId, weddingOccasionId, 105, storeId: MaadiStoreId),
            Product("014", "Candlelight Carnations", "candlelight-carnations", 470, birthdayCategoryId, birthdayOccasionId, 48),
            Product("015", "Champagne Rose Vase", "champagne-rose-vase", 990, rosesCategoryId, weddingOccasionId, 66, discountPercent: 8, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(8)),
            Product("016", "Cherry Blossom Branches", "cherry-blossom-branches", 680, birthdayCategoryId, birthdayOccasionId, 43),
            Product("017", "Coral Peony Bouquet", "coral-peony-bouquet", 860, rosesCategoryId, birthdayOccasionId, 83, storeId: NasrCityStoreId),
            Product("018", "Desert Rose Plant", "desert-rose-plant", 540, plantsCategoryId, null, 29, isAvailable: false),
            Product("019", "Elegant Eucalyptus Vase", "elegant-eucalyptus-vase", 690, plantsCategoryId, weddingOccasionId, 57),
            Product("020", "Forever Pink Roses", "forever-pink-roses", 1040, rosesCategoryId, birthdayOccasionId, 124, discountPercent: 20, activeFrom: utcNow.AddDays(-5), activeTo: utcNow.AddDays(25)),
            Product("021", "Golden Sunflower Basket", "golden-sunflower-basket", 630, birthdayCategoryId, birthdayOccasionId, 74),
            Product("022", "Graceful Lily Bouquet", "graceful-lily-bouquet", 820, rosesCategoryId, weddingOccasionId, 68, storeId: MaadiStoreId),
            Product("023", "Hanging Ivy Plant", "hanging-ivy-plant", 460, plantsCategoryId, null, 35),
            Product("024", "Ivory Wedding Centerpiece", "ivory-wedding-centerpiece", 1450, rosesCategoryId, weddingOccasionId, 112, discountPercent: 15, activeFrom: utcNow.AddDays(-10), activeTo: utcNow.AddDays(18)),
            Product("025", "Jasmine Garden Basket", "jasmine-garden-basket", 730, plantsCategoryId, birthdayOccasionId, 46, storeId: NasrCityStoreId),
            Product("026", "Lavender Love Bouquet", "lavender-love-bouquet", 700, birthdayCategoryId, birthdayOccasionId, 89, discountPercent: 5, activeFrom: utcNow.AddDays(-1), activeTo: utcNow.AddDays(5)),
            Product("027", "Lemon Lime Dracaena", "lemon-lime-dracaena", 590, plantsCategoryId, null, 31),
            Product("028", "Midnight Blue Iris", "midnight-blue-iris", 775, rosesCategoryId, weddingOccasionId, 58, discountPercent: 30, activeFrom: utcNow.AddDays(2), activeTo: utcNow.AddDays(20)),
            Product("029", "Mint Garden Planter", "mint-garden-planter", 510, plantsCategoryId, birthdayOccasionId, 41, isAvailable: false),
            Product("030", "Moonlight Orchid Duo", "moonlight-orchid-duo", 1320, plantsCategoryId, weddingOccasionId, 97, storeId: MaadiStoreId, discountPercent: 12, activeFrom: utcNow.AddDays(-3), activeTo: utcNow.AddDays(12)),
            Product("031", "Nile Lotus Arrangement", "nile-lotus-arrangement", 920, birthdayCategoryId, birthdayOccasionId, 62),
            Product("032", "Olive Tree Gift", "olive-tree-gift", 1150, plantsCategoryId, weddingOccasionId, 54),
            Product("033", "Peach Gerbera Basket", "peach-gerbera-basket", 580, birthdayCategoryId, birthdayOccasionId, 70, discountPercent: 10, activeFrom: utcNow.AddDays(-12), activeTo: utcNow.AddDays(-3)),
            Product("034", "Pearl Rose Heart", "pearl-rose-heart", 980, rosesCategoryId, weddingOccasionId, 103, storeId: NasrCityStoreId),
            Product("035", "Pink Tulip Melody", "pink-tulip-melody", 640, birthdayCategoryId, birthdayOccasionId, 79),
            Product("036", "Royal Purple Orchid", "royal-purple-orchid", 1280, plantsCategoryId, weddingOccasionId, 85, discountPercent: 17.5m, activeFrom: utcNow.AddDays(-6), activeTo: utcNow.AddDays(19)),
            Product("037", "Ruby Red Bouquet", "ruby-red-bouquet", 720, rosesCategoryId, birthdayOccasionId, 110, storeId: MaadiStoreId),
            Product("038", "Silver Leaf Plant", "silver-leaf-plant", 490, plantsCategoryId, null, 37),
            Product("039", "Summer Meadow Bouquet", "summer-meadow-bouquet", 760, birthdayCategoryId, birthdayOccasionId, 93, discountPercent: 22, activeFrom: utcNow.AddDays(-2), activeTo: utcNow.AddDays(15)),
            Product("040", "White Lily Wedding Vase", "white-lily-wedding-vase", 1180, rosesCategoryId, weddingOccasionId, 101, isAvailable: false)
        ];
    }

    private static Product Product(
        string idSuffix,
        string name,
        string imageName,
        decimal price,
        Guid categoryId,
        Guid? occasionId,
        int soldCount,
        Guid? storeId = null,
        bool isAvailable = true,
        decimal? discountPercent = null,
        DateTime? activeFrom = null,
        DateTime? activeTo = null)
        => new()
        {
            Id = Guid.Parse($"40000000-0000-0000-0000-000000000{idSuffix}"),
            Name = name,
            ImageUrl = $"/images/products/{imageName}.jpg",
            Price = price,
            DiscountPercent = discountPercent,
            DiscountStartsAtUtc = activeFrom,
            DiscountEndsAtUtc = activeTo,
            CategoryId = categoryId,
            OccasionId = occasionId,
            StoreId = storeId,
            IsAvailable = isAvailable,
            SoldCount = soldCount
        };
}
