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
    private const string CategoryRail = "category_rail";
    private const string ProductRail = "product_rail";
    private const string OccasionRail = "occasion_rail";
    private const string Banner = "banner";

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
            object? payload = section.Type switch
            {
                CategoryRail => section.Enabled
                    ? await BuildCategoryRailAsync(section.ContentRefJson, cancellationToken)
                    : null,
                ProductRail => section.Enabled
                    ? await BuildProductRailAsync(section.ContentRefJson, request.StoreId, cancellationToken)
                    : null,
                OccasionRail => section.Enabled
                    ? await BuildOccasionRailAsync(section.ContentRefJson, cancellationToken)
                    : null,
                Banner => section.Enabled
                    ? await BuildBannerAsync(section.ContentRefJson, request.StoreId, cancellationToken)
                    : null,
                _ => null
            };

            response.Add(new HomeSectionResponse(
                section.Type,
                section.Id,
                section.Title,
                section.Order,
                section.Enabled,
                payload));
        }

        return OperationResultFactory.Success<IReadOnlyList<HomeSectionResponse>>(response);
    }

    private async Task<RailPayload<CategorySummary>> BuildCategoryRailAsync(
        string contentRefJson,
        CancellationToken cancellationToken)
    {
        var config = ReadConfig<RailConfig>(contentRefJson);
        var query = unitOfWork.Repository<Category, Guid>()
            .Query()
            .Where(category => category.IsActive);

        if (config.Ids.Count > 0)
        {
            query = query.Where(category => config.Ids.Contains(category.Id));
        }

        var categories = await query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Take(config.Take)
            .Select(category => new CategorySummary(
                category.Id,
                category.Name,
                category.ImageUrl,
                $"/categories/{category.Id}"))
            .ToListAsync(cancellationToken);

        return new RailPayload<CategorySummary>(
            categories,
            new ViewAllAction("View All", config.DeepLink ?? "/categories"));
    }

    private async Task<RailPayload<ProductSummary>> BuildProductRailAsync(
        string contentRefJson,
        Guid? storeId,
        CancellationToken cancellationToken)
    {
        var config = ReadConfig<ProductRailConfig>(contentRefJson);
        var query = unitOfWork.Repository<Product, Guid>()
            .Query()
            .Where(product => product.IsActive);

        if (storeId is not null)
        {
            query = query.Where(product => product.StoreInventories.Any(inventory =>
                inventory.StoreId == storeId
                && inventory.IsEnabled
                && inventory.AvailableQuantity > 0));
        }

        if (config.Ids.Count > 0)
        {
            query = query.Where(product => config.Ids.Contains(product.Id));
        }

        query = config.SelectionRule.Equals("best_sellers", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(product => product.SoldCount).ThenBy(product => product.Name)
            : query.OrderBy(product => product.Name);

        var products = await query
            .Take(config.Take)
            .Select(product => new ProductSummary(
                product.Id,
                product.Name,
                product.ImageUrl,
                product.Price,
                product.CategoryId,
                product.OccasionId,
                $"/products/{product.Id}"))
            .ToListAsync(cancellationToken);

        return new RailPayload<ProductSummary>(
            products,
            new ViewAllAction("View All", config.DeepLink ?? "/products?sort=best_sellers"));
    }

    private async Task<RailPayload<OccasionSummary>> BuildOccasionRailAsync(
        string contentRefJson,
        CancellationToken cancellationToken)
    {
        var config = ReadConfig<RailConfig>(contentRefJson);
        var query = unitOfWork.Repository<Occasion, Guid>()
            .Query()
            .Where(occasion => occasion.IsActive);

        if (config.Ids.Count > 0)
        {
            query = query.Where(occasion => config.Ids.Contains(occasion.Id));
        }

        var occasions = await query
            .OrderBy(occasion => occasion.SortOrder)
            .ThenBy(occasion => occasion.Name)
            .Take(config.Take)
            .Select(occasion => new OccasionSummary(
                occasion.Id,
                occasion.Name,
                occasion.ImageUrl,
                $"/occasions/{occasion.Id}"))
            .ToListAsync(cancellationToken);

        return new RailPayload<OccasionSummary>(
            occasions,
            new ViewAllAction("View All", config.DeepLink ?? "/occasions"));
    }

    private async Task<BannerPayload?> BuildBannerAsync(
        string contentRefJson,
        Guid? storeId,
        CancellationToken cancellationToken)
    {
        var config = ReadConfig<BannerConfig>(contentRefJson);
        if (!string.IsNullOrWhiteSpace(config.ImageUrl))
        {
            return new BannerPayload(config.ImageUrl, config.DeepLink ?? string.Empty);
        }

        var query = unitOfWork.Repository<Banner, Guid>()
            .Query()
            .Where(banner => banner.IsActive);

        if (storeId is not null)
        {
            query = query.Where(banner => banner.StoreId == null || banner.StoreId == storeId);
        }

        if (config.BannerId is not null)
        {
            query = query.Where(banner => banner.Id == config.BannerId);
        }

        return await query
            .OrderBy(banner => banner.SortOrder)
            .Select(banner => new BannerPayload(banner.ImageUrl, banner.DeepLink))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static T ReadConfig<T>(string contentRefJson) where T : new()
        => string.IsNullOrWhiteSpace(contentRefJson)
            ? new T()
            : JsonSerializer.Deserialize<T>(contentRefJson, JsonOptions) ?? new T();

    private class RailConfig
    {
        public List<Guid> Ids { get; set; } = [];
        public int Take { get; set; } = 10;
        public string? DeepLink { get; set; }
    }

    private sealed class ProductRailConfig : RailConfig
    {
        public string SelectionRule { get; set; } = "best_sellers";
    }

    private sealed class BannerConfig
    {
        public Guid? BannerId { get; set; }
        public string? ImageUrl { get; set; }
        public string? DeepLink { get; set; }
    }
}
