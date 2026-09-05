using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Contracts.Orders;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
