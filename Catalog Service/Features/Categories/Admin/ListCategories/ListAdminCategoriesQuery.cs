using Catalog_Service.Contracts.Categories;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.ListCategories;

public sealed record ListAdminCategoriesQuery(bool IncludeArchived, string? Search)
    : IRequest<OperationResult<IReadOnlyList<AdminCategoryResponse>>>;
