using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Addresses.Delete;

public sealed record DeleteAddressCommand(
    Guid AddressId,
    string UserId) : IRequest<OperationResult>;
