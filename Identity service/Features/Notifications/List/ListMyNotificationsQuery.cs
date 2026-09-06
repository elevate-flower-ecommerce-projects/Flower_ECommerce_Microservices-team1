using Flower.Common.StandardizedResponse;
namespace Identity_service.Features.Notifications.List;

public sealed record ListMyNotificationsQuery(
    string UserId,
    int Page,
    int PageSize,
    bool? IsRead) : IRequest<OperationResult<PagedResponse<NotificationListItemResponse>>>;
