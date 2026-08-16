using Microsoft.AspNetCore.Authorization;

namespace Catalog_Service.Features.Categories.Admin;

/// <summary>
/// Shared route conventions for the administrator category endpoints.
/// </summary>
internal static class AdminCategoryRoutes
{
    public const string BasePath = "/admin/categories";
    public const string Tag = "Admin Categories";
    public const string AdminRole = "Admin";

    public static RouteGroupBuilder MapAdminCategoryGroup(this IEndpointRouteBuilder app)
        => app.MapGroup(BasePath)
            .WithTags(Tag)
            .RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole });
}
