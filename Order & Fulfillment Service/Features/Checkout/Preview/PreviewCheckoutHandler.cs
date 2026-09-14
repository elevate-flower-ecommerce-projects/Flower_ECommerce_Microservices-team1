using Flower.Common.StandardizedResponse;
using MediatR;

namespace Order___Fulfillment_Service.Features.Checkout.Preview;

public sealed class PreviewCheckoutHandler(ICheckoutQuoteBuilder quoteBuilder)
    : IRequestHandler<PreviewCheckoutQuery, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(PreviewCheckoutQuery request, CancellationToken cancellationToken)
    {
        var errors = CheckoutValidator.ValidateDelivery(request.AddressId, request.Gift);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(errors, CheckoutMessages.ValidationFailed, CheckoutMessages.ValidationFailed);

        var result = await quoteBuilder.BuildAsync(request.AddressId, request.Gift, cancellationToken);
        if (result.Failure is not null)
            return result.Failure;

        return OperationResultFactory.Success<object>(
            result.Quote!.ToSummary(),
            CheckoutMessages.SummaryReady,
            CheckoutMessages.SummaryReady);
    }
}
