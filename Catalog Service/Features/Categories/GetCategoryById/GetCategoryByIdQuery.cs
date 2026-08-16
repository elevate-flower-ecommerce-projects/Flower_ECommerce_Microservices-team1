using Catalog_Service.Contracts.Categories;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid CategoryId)
    : IRequest<OperationResult<CategoryResponse>>;
