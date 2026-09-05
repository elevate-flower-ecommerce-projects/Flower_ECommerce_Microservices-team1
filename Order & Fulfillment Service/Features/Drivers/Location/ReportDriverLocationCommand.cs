using Flower.Common.StandardizedResponse;
using MediatR;

namespace Order___Fulfillment_Service.Features.Drivers.Location;

public sealed record ReportDriverLocationCommand(Guid OrderId, string DriverUserId, decimal Latitude, decimal Longitude)
    : IRequest<OperationResult>;
