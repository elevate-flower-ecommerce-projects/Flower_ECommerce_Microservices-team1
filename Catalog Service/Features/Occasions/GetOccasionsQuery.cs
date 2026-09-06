using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Occasions;

public sealed record GetOccasionsQuery : IRequest<OperationResult<IReadOnlyList<OccasionResponse>>>;

public sealed class GetOccasionsQueryHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetOccasionsQuery, OperationResult<IReadOnlyList<OccasionResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<OccasionResponse>>> Handle(
        GetOccasionsQuery request,
        CancellationToken cancellationToken)
    {
        var occasions = await unitOfWork.Repository<Occasion, Guid>()
            .Query()
            .Where(occasion => occasion.IsActive)
            .OrderBy(occasion => occasion.SortOrder)
            .ThenBy(occasion => occasion.Name)
            .Select(occasion => new OccasionResponse(
                occasion.Id,
                occasion.Name,
                occasion.ImageUrl,
                occasion.SortOrder))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<OccasionResponse>>(occasions);
    }
}
