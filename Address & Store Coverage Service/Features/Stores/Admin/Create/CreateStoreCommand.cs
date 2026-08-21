using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Create;

public sealed record CreateStoreCommand(
    string Name,
    string Location,
    decimal? Lat,
    decimal? Lng,
    IReadOnlyList<CreateStoreCoverageArea> CoverageAreas) : IRequest<OperationResult<object>>;

public sealed record CreateStoreCoverageArea(
    string City,
    string Area,
    decimal? MinLat,
    decimal? MaxLat,
    decimal? MinLng,
    decimal? MaxLng);
