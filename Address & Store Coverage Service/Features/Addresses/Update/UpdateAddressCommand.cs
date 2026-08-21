using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Addresses.Update;

public sealed record UpdateAddressCommand(
    Guid AddressId,
    string UserId,
    string RecipientName,
    string Phone,
    string AddressLine,
    string City,
    string Area,
    decimal? Lat,
    decimal? Lng,
    string? Label) : IRequest<OperationResult<object>>;
