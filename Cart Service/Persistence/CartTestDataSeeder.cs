using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Persistence;

/// <summary>
/// Development-only test data for checkout: the seeded customer scrum23.addresses@flower.local
/// (fixed id, saved addresses in Nasr City and Maadi) gets a ready cart whenever the service starts
/// and their cart is empty. The products are stocked at both seeded stores.
/// </summary>
public static class CartTestDataSeeder
{
    private const string CheckoutTestUserId = "30000000-0000-0000-0000-000000000023";

    private static readonly (Guid ProductId, string Name, string ImageUrl, decimal UnitPrice, int Quantity)[] Lines =
    [
        (Guid.Parse("40000000-0000-0000-0000-000000000008"), "Amber Rose Bouquet", "https://images.unsplash.com/photo-1563241527-3004b7be0ffd?auto=format&fit=crop&w=800&q=80", 560m, 2),
        (Guid.Parse("40000000-0000-0000-0000-000000000010"), "Baby Breath Cloud", "https://images.unsplash.com/photo-1533616688419-b7a58556458e?auto=format&fit=crop&w=800&q=80", 430m, 1)
    ];

    public static async Task SeedAsync(CartDbContext context, CancellationToken cancellationToken = default)
    {
        var cart = await context.Carts
            .Include(existing => existing.Items)
            .SingleOrDefaultAsync(existing => existing.UserId == CheckoutTestUserId, cancellationToken);

        if (cart is { Items.Count: > 0 })
            return;

        var now = DateTime.UtcNow;
        if (cart is null)
        {
            cart = new Entities.Cart { Id = Guid.CreateVersion7(), UserId = CheckoutTestUserId, CreatedAtUtc = now };
            context.Carts.Add(cart);
        }

        foreach (var (line, index) in Lines.Select((line, index) => (line, index)))
        {
            context.CartItems.Add(new Entities.CartItem
            {
                Id = Guid.CreateVersion7(),
                CartId = cart.Id,
                ProductId = line.ProductId,
                ProductName = line.Name,
                ImageUrl = line.ImageUrl,
                UnitPriceSnapshot = line.UnitPrice,
                Quantity = line.Quantity,
                AddedAtUtc = now.AddSeconds(index)
            });
        }

        cart.UpdatedAtUtc = now;
        await context.SaveChangesAsync(cancellationToken);
    }
}
