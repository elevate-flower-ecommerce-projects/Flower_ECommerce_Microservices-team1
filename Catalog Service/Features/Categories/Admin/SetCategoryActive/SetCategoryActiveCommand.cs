using Catalog_Service.Contracts.Categories;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Catalog_Service.Features.Categories.Admin.SetCategoryActive;

/// <summary>
/// Archive and restore are the same state change in opposite directions,
/// so they share one command instead of two near-identical handlers.
/// </summary>
public sealed record SetCategoryActiveCommand(Guid CategoryId, bool IsActive)
    : IRequest<OperationResult<AdminCategoryResponse>>;
