namespace Order___Fulfillment_Service.Entities;

public sealed class DriverLocation
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OrderId { get; set; }
    public string DriverUserId { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
    public Order? Order { get; set; }
}
