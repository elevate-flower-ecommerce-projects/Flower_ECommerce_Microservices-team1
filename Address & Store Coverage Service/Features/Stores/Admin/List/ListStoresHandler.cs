using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Stores.Admin;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.List;

public sealed class ListStoresHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<ListStoresQuery, OperationResult<IReadOnlyList<StoreResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<StoreResponse>>> Handle(
        ListStoresQuery request,
        CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<Store, Guid>()
            .Query()
            .Include(store => store.CoverageAreas)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(store => store.IsActive);

        var stores = await query
            .OrderBy(store => store.Name)
            .ThenBy(store => store.Id)
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<StoreResponse>>(
            stores.Select(StoreMapping.ToResponse).ToList());
    }
}
