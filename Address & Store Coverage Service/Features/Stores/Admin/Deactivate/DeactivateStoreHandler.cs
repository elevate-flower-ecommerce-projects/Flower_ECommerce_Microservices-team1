using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Deactivate;

public sealed class DeactivateStoreHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<DeactivateStoreCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(DeactivateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await unitOfWork.Repository<Store, Guid>()
            .Query()
            .Include(store => store.CoverageAreas)
            .SingleOrDefaultAsync(store => store.Id == request.StoreId, cancellationToken);

        if (store is null)
            return OperationResultFactory.NotFound<object>(message: "Store was not found.");

        if (!store.IsActive)
            return OperationResultFactory.Conflict<object>(message: "Store is already inactive.");

        store.IsActive = false;
        store.UpdatedAtUtc = DateTime.UtcNow;
        foreach (var coverage in store.CoverageAreas)
        {
            coverage.IsActive = false;
            await unitOfWork.Repository<StoreCoverageArea, Guid>().Update(coverage);
        }

        var affectedAddresses = await unitOfWork.Repository<UserAddress, Guid>()
            .Query()
            .Where(address => address.ServingStoreId == request.StoreId)
            .ToListAsync(cancellationToken);

        foreach (var address in affectedAddresses)
        {
            address.ServingStoreId = null;
            address.IsServiceable = false;
            await unitOfWork.Repository<UserAddress, Guid>().Update(address);
        }

        await unitOfWork.Repository<Store, Guid>().Update(store);
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Success<object>(
            new { store.Id, FlaggedAddressCount = affectedAddresses.Count },
            "Store deactivated successfully.",
            "Store deactivated successfully.");
    }
}
