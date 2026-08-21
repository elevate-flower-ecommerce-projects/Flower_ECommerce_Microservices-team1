using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Stores.Admin;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Get;

public sealed class GetStoreHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<GetStoreQuery, OperationResult<StoreResponse>>
{
    public async Task<OperationResult<StoreResponse>> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        var store = await unitOfWork.Repository<Store, Guid>()
            .Query()
            .Include(store => store.CoverageAreas)
            .SingleOrDefaultAsync(store => store.Id == request.StoreId, cancellationToken);

        return store is null
            ? OperationResultFactory.NotFound<StoreResponse>(message: "Store was not found.")
            : OperationResultFactory.Success(StoreMapping.ToResponse(store));
    }
}
