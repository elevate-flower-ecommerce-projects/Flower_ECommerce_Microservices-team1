using System.Globalization;
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
    public async Task<OperationResult<IReadOnlyList<HomeSectionResponse>>> Handle(
        GetHomeLayoutQuery request,
        CancellationToken cancellationToken)
    {
        var isArabic = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase)
            || CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

        var sections = await unitOfWork.Repository<HomeSection, Guid>()
            .Query()
            .Where(section => section.Enabled)
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id)
            .ToListAsync(cancellationToken);

        var response = new List<HomeSectionResponse>(sections.Count);

        foreach (var section in sections)
        {
            Guid? occasionId = section.OccasionId;
            Guid? categoryId = section.CategoryId;
            string? titleAr = section.TitleAr;

            if ((occasionId == null || categoryId == null || string.IsNullOrWhiteSpace(titleAr)) && !string.IsNullOrWhiteSpace(section.ContentRefJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(section.ContentRefJson);
                    var root = doc.RootElement;
                    if (occasionId == null && root.TryGetProperty("occasionId", out var occProp) && occProp.ValueKind == JsonValueKind.String && Guid.TryParse(occProp.GetString(), out var parsedOcc))
                    {
                        occasionId = parsedOcc;
                    }
                    if (categoryId == null && root.TryGetProperty("categoryId", out var catProp) && catProp.ValueKind == JsonValueKind.String && Guid.TryParse(catProp.GetString(), out var parsedCat))
                    {
                        categoryId = parsedCat;
                    }
                    if (string.IsNullOrWhiteSpace(titleAr) && root.TryGetProperty("titleAr", out var titleArProp) && titleArProp.ValueKind == JsonValueKind.String)
                    {
                        titleAr = titleArProp.GetString();
                    }
                }
                catch
                {
                    // Ignore parsing error for legacy records
                }
            }

            var type = section.Type switch
            {
                "category_rail" or "categories" or "Categories" => "Categories",
                "product_rail" or "best_sellers" or "BestSeller" or "BestSellers" => "BestSeller",
                "occasion_rail" or "occasions" or "Occasions" => "Occasions",
                "carousel" or "ProductsCarousel" => "ProductsCarousel",
                _ => section.Type
            };

            var title = isArabic && !string.IsNullOrWhiteSpace(titleAr)
                ? titleAr
                : (section.Title ?? string.Empty);

            response.Add(new HomeSectionResponse(
                section.Id,
                type,
                title,
                section.Order,
                section.Enabled,
                occasionId,
                categoryId));
        }

        return OperationResultFactory.Success<IReadOnlyList<HomeSectionResponse>>(response);
    }
}
