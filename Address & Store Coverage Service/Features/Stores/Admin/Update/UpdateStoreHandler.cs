using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Stores.Admin;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Update;

public sealed class UpdateStoreHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<UpdateStoreCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var storeRequest = new StoreRequest(
            request.Name,
            request.Location,
            request.Lat,
            request.Lng,
            request.CoverageAreas
                .Select(area => new StoreCoverageAreaRequest(area.City, area.Area, area.MinLat, area.MaxLat, area.MinLng, area.MaxLng))
                .ToList());

        var errors = StoreRequestValidator.Validate(storeRequest);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(errors, "Store validation failed.", "Store validation failed.");

        var storeRepository = unitOfWork.Repository<Store, Guid>();
        var coverageRepository = unitOfWork.Repository<StoreCoverageArea, Guid>();
        var store = await storeRepository
            .Query()
            .Include(store => store.CoverageAreas)
            .SingleOrDefaultAsync(store => store.Id == request.StoreId, cancellationToken);

        if (store is null)
            return OperationResultFactory.NotFound<object>(message: "Store was not found.");

        var normalizedName = request.Name.Trim().ToUpperInvariant();
        if (await storeRepository.Query().AnyAsync(
            store => store.Id != request.StoreId && store.Name.ToUpper() == normalizedName,
            cancellationToken))
        {
            return OperationResultFactory.Conflict<object>(
                new Dictionary<string, string[]> { [nameof(request.Name)] = ["A store with this name already exists."] },
                "Store name already exists.",
                "Store name already exists.");
        }

        foreach (var coverage in store.CoverageAreas.Where(coverage => coverage.IsActive))
        {
            coverage.IsActive = false;
            await coverageRepository.Update(coverage);
        }

        var newCoverageAreas = request.CoverageAreas.Select(area => new StoreCoverageArea
        {
            Id = Guid.CreateVersion7(),
            StoreId = store.Id,
            City = area.City.Trim(),
            Area = area.Area.Trim(),
            MinLat = area.MinLat,
            MaxLat = area.MaxLat,
            MinLng = area.MinLng,
            MaxLng = area.MaxLng,
            IsActive = true
        }).ToList();

        foreach (var coverage in newCoverageAreas)
        {
            await coverageRepository.Create(coverage);
        }

        store.Name = request.Name.Trim();
        store.Location = request.Location.Trim();
        store.Lat = request.Lat;
        store.Lng = request.Lng;
        store.UpdatedAtUtc = DateTime.UtcNow;

        await storeRepository.Update(store);
        await unitOfWork.CompleteAsync();

        store.CoverageAreas = newCoverageAreas;

        return OperationResultFactory.Success<object>(
            StoreMapping.ToResponse(store),
            "Store updated successfully.",
            "Store updated successfully.");
    }
}
