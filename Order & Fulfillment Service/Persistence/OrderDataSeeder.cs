using Microsoft.EntityFrameworkCore;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Persistence;

public interface IOrderDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class OrderDataSeeder(
    OrderDbContext dbContext,
    IHostEnvironment environment) : IOrderDataSeeder
{
    private const string Scrum23TestUserId = "30000000-0000-0000-0000-000000000023";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
            return;

        foreach (var order in BuildSeedOrders())
        {
            if (await dbContext.Orders.AnyAsync(existing => existing.Id == order.Id, cancellationToken))
                continue;

            dbContext.Orders.Add(order);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<Order> BuildSeedOrders() =>
    [
        new Order
        {
            Id = Guid.Parse("91000000-0000-0000-0000-000000000001"),
            UserId = Scrum23TestUserId,
            OrderNumber = "FL-20260830-001",
            PlacedAtUtc = new DateTime(2026, 8, 30, 10, 45, 0, DateTimeKind.Utc),
            Status = OrderStatus.OutForDelivery,
            DeliveryRecipientName = "SCRUM-23 Test Customer",
            DeliveryPhone = "01012345678",
            DeliveryAddressLine = "23 Abbas El Akkad Street, Building 4",
            DeliveryCity = "Cairo",
            DeliveryArea = "Nasr City",
            IsGift = true,
            GiftRecipientName = "Mona Ali",
            GiftRecipientPhone = "01055550000",
            GiftMessage = "Happy birthday",
            PaymentMethod = PaymentMethodType.Card,
            PaymentLast4 = "4242",
            Subtotal = 1450m,
            DeliveryFee = 50m,
            Discount = 100m,
            Total = 1400m,
            Items =
            [
                new OrderLineItem
                {
                    Id = Guid.Parse("92000000-0000-0000-0000-000000000001"),
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    ProductName = "Red Rose Bouquet",
                    ThumbnailUrl = "/images/products/red-rose-bouquet.jpg",
                    Quantity = 1,
                    UnitPrice = 950m,
                    LineTotal = 950m
                },
                new OrderLineItem
                {
                    Id = Guid.Parse("92000000-0000-0000-0000-000000000002"),
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    ProductName = "Chocolate Add-on",
                    ThumbnailUrl = "/images/products/chocolate-addon.jpg",
                    Quantity = 1,
                    UnitPrice = 500m,
                    LineTotal = 500m
                }
            ]
        },
        new Order
        {
            Id = Guid.Parse("91000000-0000-0000-0000-000000000002"),
            UserId = Scrum23TestUserId,
            OrderNumber = "FL-20260825-004",
            PlacedAtUtc = new DateTime(2026, 8, 25, 14, 20, 0, DateTimeKind.Utc),
            Status = OrderStatus.Delivered,
            DeliveryRecipientName = "SCRUM-23 Test Customer",
            DeliveryPhone = "01012345678",
            DeliveryAddressLine = "9 Road 9, near Maadi Metro Station",
            DeliveryCity = "Cairo",
            DeliveryArea = "Maadi",
            PaymentMethod = PaymentMethodType.CashOnDelivery,
            Subtotal = 780m,
            DeliveryFee = 45m,
            Discount = 0m,
            Total = 825m,
            Items =
            [
                new OrderLineItem
                {
                    Id = Guid.Parse("92000000-0000-0000-0000-000000000003"),
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                    ProductName = "Peace Lily Plant",
                    ThumbnailUrl = "/images/products/peace-lily.jpg",
                    Quantity = 1,
                    UnitPrice = 780m,
                    LineTotal = 780m
                }
            ]
        }
    ];
}