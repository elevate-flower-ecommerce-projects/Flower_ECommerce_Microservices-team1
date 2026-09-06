namespace Identity_service.Features.Notifications.List;

public sealed record NotificationListItemResponse(
    Guid Id,
    string Title,
    string Body,
    string Type,
    string? DeepLink,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);