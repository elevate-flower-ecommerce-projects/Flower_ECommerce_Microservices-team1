using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Checkout;

namespace Order___Fulfillment_Service.Features.Checkout.Preview;

public sealed record PreviewCheckoutQuery(Guid? AddressId, GiftDetailsRequest? Gift) : IRequest<OperationResult<object>>;
