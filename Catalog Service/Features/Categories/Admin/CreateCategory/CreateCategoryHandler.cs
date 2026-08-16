using Catalog_Service.Contracts.Categories;
using Catalog_Service.Entities;
using Catalog_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.Categories.Admin.CreateCategory;

/// <summary>
/// Adds a category. New categories appear on the customer bar immediately,
/// which is what keeps clients in sync without a release.
/// </summary>
public sealed class CreateCategoryHandler(
    CatalogDbContext dbContext,
    ILogger<CreateCategoryHandler> logger)
    : IRequestHandler<CreateCategoryCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var errors = CategoryInputValidator.Validate(request.Name, request.ImageUrl, request.SortOrder);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(
                errors,
                CategoryMessages.ValidationFailed,
                CategoryMessages.ValidationFailed);

        var name = request.Name.Trim();

        if (await dbContext.Categories.AnyAsync(category => category.Name == name, cancellationToken))
            return OperationResultFactory.Conflict<object>(
                message: CategoryMessages.NameAlreadyExists,
                messageLocalized: CategoryMessages.NameAlreadyExists);

        var category = new Category
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            SortOrder = request.SortOrder ?? 0,
            IsActive = request.IsActive ?? true
        };

        dbContext.Categories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        // The AnyAsync check above loses to a concurrent insert; the unique index does not.
        catch (DbUpdateException exception) when (IsDuplicateName(exception))
        {
            logger.LogWarning("Category name {Name} already exists.", name);
            return OperationResultFactory.Conflict<object>(
                message: CategoryMessages.NameAlreadyExists,
                messageLocalized: CategoryMessages.NameAlreadyExists);
        }

        return OperationResultFactory.Created<object>(
            new AdminCategoryResponse(
                category.Id,
                category.Name,
                category.ImageUrl,
                category.SortOrder,
                category.IsActive),
            CategoryMessages.Created,
            CategoryMessages.Created);
    }

    private static bool IsDuplicateName(DbUpdateException exception)
        => (exception.InnerException?.Message ?? exception.Message)
            .Contains("UX_Category_Name", StringComparison.OrdinalIgnoreCase);
}
