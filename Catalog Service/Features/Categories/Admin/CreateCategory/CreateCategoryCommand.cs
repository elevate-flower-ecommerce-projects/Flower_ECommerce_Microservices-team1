using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.CreateCategory;

// Object payload: a success carries AdminCategoryResponse, a failure carries the
// field-keyed validation errors. The endpoint documents both shapes.
public sealed record CreateCategoryCommand(
    string Name,
    string? ImageUrl,
    int? SortOrder,
    bool? IsActive) : IRequest<OperationResult<object>>;
