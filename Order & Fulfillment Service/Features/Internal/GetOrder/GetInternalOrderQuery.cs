using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Internal;

namespace Order___Fulfillment_Service.Features.Internal.GetOrder;

public sealed record GetInternalOrderQuery(Guid OrderId) : IRequest<OperationResult<InternalOrderResponse>>;
