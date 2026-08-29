using Cart_Service.Persistence;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart;

public static class CartQueryExtensions
{
    /// <summary>Loads the caller's single cart with its lines, or null when they have none yet.</summary>
    public static Task<Entities.Cart?> FindCartWithItemsAsync(
        this IUnitOfWork<CartDbContext> unitOfWork,
        string userId,
        CancellationToken cancellationToken)
        => unitOfWork.Repository<Entities.Cart, Guid>()
            .Query()
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
}
