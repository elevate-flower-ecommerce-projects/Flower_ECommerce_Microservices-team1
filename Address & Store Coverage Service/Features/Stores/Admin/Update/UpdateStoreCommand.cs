using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Update;

public sealed record UpdateStoreCommand(
    Guid StoreId,
    string Name,
    string Location,
    decimal? Lat,
    decimal? Lng,
    IReadOnlyList<UpdateStoreCoverageArea> CoverageAreas) : IRequest<OperationResult<object>>;

public sealed record UpdateStoreCoverageArea(
    string City,
    string Area,
    decimal? MinLat,
    decimal? MaxLat,
    decimal? MinLng,
    decimal? MaxLng);
