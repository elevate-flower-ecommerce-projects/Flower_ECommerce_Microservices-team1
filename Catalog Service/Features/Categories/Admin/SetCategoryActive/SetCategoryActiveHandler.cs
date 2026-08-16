using Catalog_Service.Contracts.Categories;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.Admin.SetCategoryActive;

/// <summary>
/// Archives or restores a category. Archiving only hides it from the customer bar —
/// the row stays so old deep links can answer 410 Gone instead of 404.
/// </summary>
public sealed class SetCategoryActiveHandler(CatalogDbContext dbContext)
    : IRequestHandler<SetCategoryActiveCommand, OperationResult<AdminCategoryResponse>>
{
    public async Task<OperationResult<AdminCategoryResponse>> Handle(
        SetCategoryActiveCommand request,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(entity => entity.Id == request.CategoryId, cancellationToken);

        if (category is null)
            return OperationResultFactory.NotFound<AdminCategoryResponse>(
                message: CategoryMessages.NotFound,
                messageLocalized: CategoryMessages.NotFound);

        if (category.IsActive == request.IsActive)
        {
            var alreadyMessage = request.IsActive
                ? CategoryMessages.AlreadyActive
                : CategoryMessages.AlreadyArchived;

            return OperationResultFactory.Conflict<AdminCategoryResponse>(
                message: alreadyMessage,
                messageLocalized: alreadyMessage);
        }

        category.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = request.IsActive
            ? CategoryMessages.Restored
            : CategoryMessages.Archived;

        return OperationResultFactory.Success(
            new AdminCategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                category.SortOrder,
                category.IsActive),
            message,
            message);
    }
}
