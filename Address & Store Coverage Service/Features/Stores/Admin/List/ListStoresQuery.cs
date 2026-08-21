using Address___Store_Coverage_Service.Contracts.Stores;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.List;

public sealed record ListStoresQuery(bool IncludeInactive) : IRequest<OperationResult<IReadOnlyList<StoreResponse>>>;
