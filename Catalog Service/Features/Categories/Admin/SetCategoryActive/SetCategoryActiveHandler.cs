using Catalog_Service.Contracts.Categories;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Repository.Layer.Interfaces;

namespace Catalog_Service.Features.Categories.Admin.SetCategoryActive;

/// <summary>
/// Archives or restores a category. Archiving only hides it from the customer bar,
/// the row stays so old deep links can answer 410 Gone instead of 404.
/// </summary>
public sealed class SetCategoryActiveHandler(IUnitOfWork<CatalogDbContext> unitOfWork)
    : IRequestHandler<SetCategoryActiveCommand, OperationResult<AdminCategoryResponse>>
{
    public async Task<OperationResult<AdminCategoryResponse>> Handle(
        SetCategoryActiveCommand request,
        CancellationToken cancellationToken)
    {
        var categoryRepository = unitOfWork.Repository<Category, Guid>();
        var category = await categoryRepository.Get(request.CategoryId);

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
        await categoryRepository.Update(category);
        await unitOfWork.CompleteAsync();

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
