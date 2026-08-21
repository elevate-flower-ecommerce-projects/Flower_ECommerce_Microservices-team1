using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.List;

public sealed class ListMyAddressesHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<ListMyAddressesQuery, OperationResult<IReadOnlyList<AddressListItemResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<AddressListItemResponse>>> Handle(
        ListMyAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var addresses = await unitOfWork.Repository<UserAddress, Guid>()
            .Query()
            .Where(address => address.UserId == request.UserId)
            .OrderByDescending(address => address.IsDefault)
            .ThenByDescending(address => address.LastUsedAtUtc ?? address.CreatedAtUtc)
            .ThenByDescending(address => address.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var response = addresses.Select(AddressMapping.ToListItem).ToList();

        return OperationResultFactory.Success<IReadOnlyList<AddressListItemResponse>>(
            response,
            response.Count == 0 ? "No saved addresses yet." : "Addresses loaded successfully.",
            response.Count == 0 ? "No saved addresses yet." : "Addresses loaded successfully.");
    }
}
