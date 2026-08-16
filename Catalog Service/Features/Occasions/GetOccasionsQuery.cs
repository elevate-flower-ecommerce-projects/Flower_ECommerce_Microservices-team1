using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Occasions;

public sealed record GetOccasionsQuery : IRequest<OperationResult<IReadOnlyList<OccasionResponse>>>;

public sealed class GetOccasionsQueryHandler(CatalogDbContext dbContext)
    : IRequestHandler<GetOccasionsQuery, OperationResult<IReadOnlyList<OccasionResponse>>>
{
    public async Task<OperationResult<IReadOnlyList<OccasionResponse>>> Handle(
        GetOccasionsQuery request,
        CancellationToken cancellationToken)
    {
        var occasions = await dbContext.Occasions
            .AsNoTracking()
            .Where(occasion => !occasion.IsArchived)
            .OrderBy(occasion => occasion.DisplayOrder)
            .ThenBy(occasion => occasion.Name)
            .Select(occasion => new OccasionResponse(
                occasion.Id,
                occasion.Name,
                occasion.ImageUrl,
                occasion.DisplayOrder))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<OccasionResponse>>(occasions);
    }
}
