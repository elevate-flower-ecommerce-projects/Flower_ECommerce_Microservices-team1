using Address___Store_Coverage_Service.Contracts.Addresses;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Addresses.List;

public sealed record ListMyAddressesQuery(string UserId) : IRequest<OperationResult<IReadOnlyList<AddressListItemResponse>>>;
