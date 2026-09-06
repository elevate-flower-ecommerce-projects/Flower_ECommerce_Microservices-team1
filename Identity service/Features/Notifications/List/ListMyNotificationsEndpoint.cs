using Flower.Common.StandardizedResponse;
namespace Identity_service.Features.Notifications.List;

public sealed class ListMyNotificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications", async (
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20,
            bool? isRead = null) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResultFactory.UnAuthorized(
                    "Missing user identity.",
                    "Missing user identity.")
                    .ToHttpResult();
            }

            var result = await sender.Send(new ListMyNotificationsQuery(userId, page, pageSize, isRead), cancellationToken);
            return result.ToHttpResult();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Customer" })
        .WithName("ListMyNotifications")
        .WithTags("Notifications")
        .Produces<OperationResult<PagedResponse<NotificationListItemResponse>>>()
        .Produces<OperationResult>(StatusCodes.Status401Unauthorized)
        .Produces<OperationResult>(StatusCodes.Status403Forbidden);
    }
}
