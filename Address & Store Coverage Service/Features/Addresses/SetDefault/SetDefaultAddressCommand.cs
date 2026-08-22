using Address___Store_Coverage_Service.Contracts.Addresses;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Addresses.SetDefault;

public sealed record SetDefaultAddressCommand(string UserId, Guid AddressId)
    : IRequest<OperationResult<AddressResponse>>;
