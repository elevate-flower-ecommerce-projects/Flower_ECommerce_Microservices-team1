using System.Data;
using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.SetDefault;

public sealed class SetDefaultAddressHandler(
    IUnitOfWork<AddressDbContext> unitOfWork,
    AddressDbContext dbContext)
    : IRequestHandler<SetDefaultAddressCommand, OperationResult<AddressResponse>>
{
    public async Task<OperationResult<AddressResponse>> Handle(
        SetDefaultAddressCommand request,
        CancellationToken cancellationToken)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var addressRepository = unitOfWork.Repository<UserAddress, Guid>();
            var address = await addressRepository
                .Query()
                .FirstOrDefaultAsync(
                    item => item.UserId == request.UserId && item.Id == request.AddressId,
                    cancellationToken);

            if (address is null)
                return OperationResultFactory.NotFound<AddressResponse>(message: "Address was not found.");

            await addressRepository
                .Query()
                .Where(item => item.UserId == request.UserId && item.Id != request.AddressId && item.IsDefault)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.IsDefault, false),
                    cancellationToken);

            address.IsDefault = true;
            await addressRepository.Update(address);
            await unitOfWork.CompleteAsync();
            await transaction.CommitAsync(cancellationToken);

            return OperationResultFactory.Success(
                AddressMapping.ToResponse(address),
                "Default address updated successfully.",
                "Default address updated successfully.");
        });
    }
}
