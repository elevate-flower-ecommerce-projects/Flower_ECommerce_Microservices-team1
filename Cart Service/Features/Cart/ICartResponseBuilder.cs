using Cart_Service.Contracts.Cart;

namespace Cart_Service.Features.Cart;

public interface ICartResponseBuilder
{
    /// <summary>
    /// Builds the full priced cart every endpoint returns, re-validating each stored line against
    /// live Catalog data so the client can render price/stock change indicators (SCRUM-29).
    /// </summary>
    Task<CartResponse> BuildAsync(Entities.Cart? cart, Guid? storeId, CancellationToken cancellationToken);
}
