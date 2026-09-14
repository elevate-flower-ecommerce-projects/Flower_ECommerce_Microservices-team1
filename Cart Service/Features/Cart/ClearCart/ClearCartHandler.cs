using Cart_Service.Entities;
using Cart_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Cart_Service.Features.Cart.ClearCart;

public sealed class ClearCartHandler(IUnitOfWork<CartDbContext> unitOfWork)
    : IRequestHandler<ClearCartCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await unitOfWork.FindCartWithItemsAsync(request.UserId, cancellationToken);

        // Checkout retries this call, so an empty or missing cart is already the desired outcome.
        if (cart is not null && cart.Items.Count > 0)
        {
            var itemRepository = unitOfWork.Repository<CartItem, Guid>();
            foreach (var item in cart.Items.ToList())
            {
                await itemRepository.Delete(item);
            }

            cart.UpdatedAtUtc = DateTime.UtcNow;
            await unitOfWork.CompleteAsync();
        }

        return OperationResultFactory.NoContent(CartMessages.CartCleared, CartMessages.CartCleared);
    }
}
