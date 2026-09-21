using Flower.Common.StandardizedResponse;
using MediatR;

namespace Payment_Service.Features.Payments.CreateSession;

public sealed record CreatePaymentSessionCommand(Guid OrderId, string CustomerUserId)
    : IRequest<OperationResult<object>>;
