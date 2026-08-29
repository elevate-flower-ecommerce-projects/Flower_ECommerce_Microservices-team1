using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Areas.DTOs;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Address___Store_Coverage_Service.Features.Areas.Queries;

public sealed class GetAreasWithCitiesQueryHandler(AddressDbContext dbContext)
    : IRequestHandler<GetAreasWithCitiesQuery, OperationResult<IReadOnlyList<AreaWithCitiesDto>>>
{
    public async Task<OperationResult<IReadOnlyList<AreaWithCitiesDto>>> Handle(
        GetAreasWithCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var areas = await dbContext.Areas
            .AsNoTracking()
            .Where(a => a.DeletedAt == null && a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new AreaWithCitiesDto(
                a.Id,
                a.Name,
                a.Cities
                    .Where(c => c.DeletedAt == null && c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new CityDto(c.Id, c.Name))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return OperationResultFactory.Success<IReadOnlyList<AreaWithCitiesDto>>(areas);
    }
}
