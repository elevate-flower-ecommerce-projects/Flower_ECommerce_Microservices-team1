using Address___Store_Coverage_Service.Contracts.Stores;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Diagnostics;

public sealed class GetCoverageDiagnosticsHandler(IUnitOfWork<AddressDbContext> unitOfWork)
    : IRequestHandler<GetCoverageDiagnosticsQuery, OperationResult<CoverageDiagnosticsResponse>>
{
    public async Task<OperationResult<CoverageDiagnosticsResponse>> Handle(
        GetCoverageDiagnosticsQuery request,
        CancellationToken cancellationToken)
    {
        var coverageAreas = await unitOfWork.Repository<StoreCoverageArea, Guid>()
            .Query()
            .Include(coverage => coverage.Store)
            .ToListAsync(cancellationToken);

        var activeGroups = coverageAreas
            .Where(coverage => coverage.IsActive && coverage.Store?.IsActive == true)
            .GroupBy(coverage => new { City = Normalize(coverage.City), Area = Normalize(coverage.Area) })
            .ToList();

        var overlaps = activeGroups
            .Where(group => group.Select(coverage => coverage.StoreId).Distinct().Count() > 1)
            .Select(group => new CoverageOverlapResponse(
                group.First().City,
                group.First().Area,
                group.Select(coverage => coverage.StoreId).Distinct().OrderBy(id => id).ToList()))
            .OrderBy(overlap => overlap.City)
            .ThenBy(overlap => overlap.Area)
            .ToList();

        var activeKeys = activeGroups.Select(group => group.Key).ToHashSet();
        var gaps = coverageAreas
            .GroupBy(coverage => new { City = Normalize(coverage.City), Area = Normalize(coverage.Area) })
            .Where(group => !activeKeys.Contains(group.Key))
            .Select(group => new CoverageGapResponse(group.First().City, group.First().Area))
            .OrderBy(gap => gap.City)
            .ThenBy(gap => gap.Area)
            .ToList();

        return OperationResultFactory.Success(new CoverageDiagnosticsResponse(gaps, overlaps));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
