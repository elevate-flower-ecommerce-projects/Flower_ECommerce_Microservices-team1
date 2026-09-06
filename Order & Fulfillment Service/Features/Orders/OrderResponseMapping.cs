using Order___Fulfillment_Service.Contracts.Drivers;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Orders;

public static class OrderResponseMapping
{
    public static DriverOrderSummaryResponse ToSummary(this Order order)
        => new(order.Id, order.OrderNumber, order.DeliveryArea, order.Items.Sum(item => item.Quantity), order.Status);

    public static DriverOrderDetailsResponse ToDriverDetails(this Order order)
        => new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Items.Select(item => new DriverOrderItemResponse(item.ProductName, item.Quantity, item.UnitPrice)).ToList(),
            order.RecipientName,
            order.RecipientPhone,
            order.StoreName,
            order.StoreAddress,
            new LocationResponse(order.StoreLatitude, order.StoreLongitude),
            order.DeliveryAddressLine,
            order.DeliveryCity,
            order.DeliveryArea,
            new LocationResponse(order.DeliveryLatitude, order.DeliveryLongitude));
}
