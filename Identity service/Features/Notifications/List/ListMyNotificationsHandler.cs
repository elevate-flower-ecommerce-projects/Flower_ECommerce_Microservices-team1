using Flower.Common.StandardizedResponse;
using System.Linq.Expressions;

namespace Identity_service.Features.Notifications.List;

public sealed class ListMyNotificationsHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<ListMyNotificationsQuery, OperationResult<PagedResponse<NotificationListItemResponse>>>
{
    public async Task<OperationResult<PagedResponse<NotificationListItemResponse>>> Handle(
        ListMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        Expression<Func<UserNotification, bool>> predicate = notification =>
            notification.UserId == request.UserId
            && (request.IsRead == null || notification.IsRead == request.IsRead);

        var paged = await unitOfWork.Repository<UserNotification, Guid>()
            .GetPageSelectAsync(
                page,
                pageSize,
                predicate,
                notification => new NotificationListItemResponse(
                    notification.Id,
                    notification.Title,
                    notification.Body,
                    notification.Type,
                    notification.DeepLink,
                    notification.IsRead,
                    notification.CreatedAtUtc,
                    notification.ReadAtUtc),
                query => query.OrderByDescending(notification => notification.CreatedAtUtc).ThenByDescending(notification => notification.Id));

        return OperationResultFactory.Success(
            new PagedResponse<NotificationListItemResponse>(page, pageSize, paged.TotalCount, paged.Items));
    }
}
