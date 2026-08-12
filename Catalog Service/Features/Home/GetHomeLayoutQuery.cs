using Catalog_Service.Contracts.Home;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Home;

public sealed record GetHomeLayoutQuery(Guid? StoreId)
    : IRequest<OperationResult<IReadOnlyList<HomeSectionResponse>>>;
