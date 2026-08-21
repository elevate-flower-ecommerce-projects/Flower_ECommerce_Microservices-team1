using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Stores.Admin;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Create;

public sealed class CreateStoreHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<CreateStoreCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
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

        var normalizedName = request.Name.Trim().ToUpperInvariant();
        if (await unitOfWork.Repository<Store, Guid>().Query().AnyAsync(
            store => store.Name.ToUpper() == normalizedName,
            cancellationToken))
        {
            return OperationResultFactory.Conflict<object>(
                new Dictionary<string, string[]> { [nameof(request.Name)] = ["A store with this name already exists."] },
                "Store name already exists.",
                "Store name already exists.");
        }

        var store = new Store
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            Lat = request.Lat,
            Lng = request.Lng,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CoverageAreas = request.CoverageAreas.Select(area => new StoreCoverageArea
            {
                Id = Guid.CreateVersion7(),
                City = area.City.Trim(),
                Area = area.Area.Trim(),
                MinLat = area.MinLat,
                MaxLat = area.MaxLat,
                MinLng = area.MinLng,
                MaxLng = area.MaxLng,
                IsActive = true
            }).ToList()
        };

        await unitOfWork.Repository<Store, Guid>().Create(store);
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Created<object>(
            StoreMapping.ToResponse(store),
            "Store created successfully.",
            "Store created successfully.");
    }
}
