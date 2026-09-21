using Flower.Common.StandardizedResponse;
using MediatR;
using Order___Fulfillment_Service.Contracts.Internal;

namespace Order___Fulfillment_Service.Features.Internal.MarkOrderPaid;

public sealed record MarkOrderPaidCommand(
    Guid OrderId,
    string? Provider,
    string? Reference,
    decimal? Amount) : IRequest<OperationResult<InternalOrderResponse>>;
