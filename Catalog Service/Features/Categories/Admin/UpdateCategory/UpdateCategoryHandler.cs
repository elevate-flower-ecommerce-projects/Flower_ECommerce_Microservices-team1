using Catalog_Service.Contracts.Categories;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.Admin.UpdateCategory;

/// <summary>
/// Renames a category and updates its image and position.
/// Archiving is a separate operation so a rename can never disable a category by accident.
/// </summary>
public sealed class UpdateCategoryHandler(
    CatalogDbContext dbContext,
    ILogger<UpdateCategoryHandler> logger)
    : IRequestHandler<UpdateCategoryCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var errors = CategoryInputValidator.Validate(request.Name, request.ImageUrl, request.SortOrder);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(
                errors,
                CategoryMessages.ValidationFailed,
                CategoryMessages.ValidationFailed);

        // Archived categories stay editable so they can be fixed before being restored.
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(entity => entity.Id == request.CategoryId, cancellationToken);

        if (category is null)
            return OperationResultFactory.NotFound<object>(
                message: CategoryMessages.NotFound,
                messageLocalized: CategoryMessages.NotFound);

        var name = request.Name.Trim();

        var nameTaken = await dbContext.Categories.AnyAsync(
            entity => entity.Id != request.CategoryId && entity.Name == name,
            cancellationToken);

        if (nameTaken)
            return OperationResultFactory.Conflict<object>(
                message: CategoryMessages.NameAlreadyExists,
                messageLocalized: CategoryMessages.NameAlreadyExists);

        category.Name = name;
        category.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();

        if (request.SortOrder is not null)
            category.SortOrder = request.SortOrder.Value;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateName(exception))
        {
            logger.LogWarning("Category name {Name} already exists.", name);
            return OperationResultFactory.Conflict<object>(
                message: CategoryMessages.NameAlreadyExists,
                messageLocalized: CategoryMessages.NameAlreadyExists);
        }

        return OperationResultFactory.Success<object>(
            new AdminCategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                category.SortOrder,
                category.IsActive),
            CategoryMessages.Updated,
            CategoryMessages.Updated);
    }

    private static bool IsDuplicateName(DbUpdateException exception)
        => (exception.InnerException?.Message ?? exception.Message)
            .Contains("UX_Category_Name", StringComparison.OrdinalIgnoreCase);
}
