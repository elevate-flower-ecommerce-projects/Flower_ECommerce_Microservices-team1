using Address___Store_Coverage_Service.Contracts.Stores;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Resolve;

public sealed record ResolveStoreQuery(
    string? City,
    string? Area,
    decimal? Lat,
    decimal? Lng) : IRequest<OperationResult<object>>;
