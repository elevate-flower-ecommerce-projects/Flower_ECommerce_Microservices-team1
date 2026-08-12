using Catalog_Service.Contracts.Home;
using MediatR;

namespace Catalog_Service.Features.Home;

public sealed record GetHomeLayoutQuery(Guid? StoreId)
    : IRequest<IReadOnlyList<HomeSectionResponse>>;
