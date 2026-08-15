using Catalog_Service.Contracts.Categories;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery(string? Search)
    : IRequest<OperationResult<IReadOnlyList<CategoryResponse>>>;
