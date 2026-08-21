using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.Details;

public sealed class GetMyAddressDetailsHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<GetMyAddressDetailsQuery, OperationResult<AddressResponse>>
{
    public async Task<OperationResult<AddressResponse>> Handle(
        GetMyAddressDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var address = await unitOfWork.Repository<UserAddress, Guid>()
            .Query()
            .SingleOrDefaultAsync(
                address => address.UserId == request.UserId && address.Id == request.AddressId,
                cancellationToken);

        return address is null
            ? OperationResultFactory.NotFound<AddressResponse>(message: "Address was not found.")
            : OperationResultFactory.Success(AddressMapping.ToResponse(address));
    }
}
