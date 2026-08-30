namespace Order___Fulfillment_Service.Contracts.Orders;

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);