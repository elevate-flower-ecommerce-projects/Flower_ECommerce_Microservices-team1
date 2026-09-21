using Flower.Common.StandardizedResponse;
using MediatR;

namespace Payment_Service.Features.Payments.GetStatus;

public sealed record GetPaymentStatusQuery(Guid OrderId, string CustomerUserId)
    : IRequest<OperationResult<object>>;
