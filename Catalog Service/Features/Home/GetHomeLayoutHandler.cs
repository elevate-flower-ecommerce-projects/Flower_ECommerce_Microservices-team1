using System.Text.Json;
using Catalog_Service.Contracts.Home;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Home;

public sealed class GetHomeLayoutHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<GetHomeLayoutQuery, OperationResult<IReadOnlyList<HomeSectionResponse>>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OperationResult<IReadOnlyList<HomeSectionResponse>>> Handle(
        GetHomeLayoutQuery request,
        CancellationToken cancellationToken)
    {
        var sections = await unitOfWork.Repository<HomeSection, Guid>()
            .Query()
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id)
            .ToListAsync(cancellationToken);

        var response = new List<HomeSectionResponse>(sections.Count);

        foreach (var section in sections)
        {
            response.Add(new HomeSectionResponse(
                section.Type,
                section.Id,
                section.Title,
                section.Order,
                section.Enabled,
                ReadPayload(section.ContentRefJson)));
        }

        return OperationResultFactory.Success<IReadOnlyList<HomeSectionResponse>>(response);
    }

    private static object? ReadPayload(string contentRefJson)
        => string.IsNullOrWhiteSpace(contentRefJson)
            ? null
            : JsonSerializer.Deserialize<JsonElement>(contentRefJson, JsonOptions);
}
