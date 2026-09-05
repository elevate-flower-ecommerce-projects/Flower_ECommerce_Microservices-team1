namespace Order___Fulfillment_Service.Contracts.Drivers;

public sealed record DriverLocationRequest(Guid OrderId, decimal Latitude, decimal Longitude);
