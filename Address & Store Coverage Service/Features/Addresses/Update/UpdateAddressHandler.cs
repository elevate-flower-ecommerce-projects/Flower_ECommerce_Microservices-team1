using Address___Store_Coverage_Service.Contracts.Addresses;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Features.Addresses.Create;
using Address___Store_Coverage_Service.Persistence;
using Address___Store_Coverage_Service.Services.GeoLookup;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.Update;

public sealed class UpdateAddressHandler(
    IUnitOfWork<AddressDbContext> unitOfWork,
    IGeoLookupService geoLookupService,
    ICreateAddressValidator validator) : IRequestHandler<UpdateAddressCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken)
    {
        var errors = validator.Validate(new CreateAddressCommand(
            request.UserId,
            request.RecipientName,
            request.Phone,
            request.AddressLine,
            request.City,
            request.Area,
            request.Lat,
            request.Lng,
            request.Label));

        if (errors.Count > 0)
        {
            return OperationResultFactory.Validation<object>(
                errors,
                "Address validation failed.",
                "Address validation failed.");
        }

        var addressRepository = unitOfWork.Repository<UserAddress, Guid>();
        var address = await addressRepository
            .Query()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.AddressId && candidate.UserId == request.UserId,
                cancellationToken);

        if (address is null)
        {
            return OperationResultFactory.NotFound<object>(
                message: "Address was not found.",
                messageLocalized: "Address was not found.");
        }

        var recipientName = request.RecipientName.Trim();
        var phone = request.Phone.Trim();
        var addressLine = request.AddressLine.Trim();
        var city = request.City.Trim();
        var area = request.Area.Trim();
        var label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim();

        var requiresStoreResolution =
            !string.Equals(address.AddressLine, addressLine, StringComparison.Ordinal) ||
            !string.Equals(address.City, city, StringComparison.Ordinal) ||
            !string.Equals(address.Area, area, StringComparison.Ordinal) ||
            address.Lat != request.Lat ||
            address.Lng != request.Lng;

        address.RecipientName = recipientName;
        address.Phone = phone;
        address.AddressLine = addressLine;
        address.City = city;
        address.Area = area;
        address.Lat = request.Lat;
        address.Lng = request.Lng;
        address.Label = label;

        if (requiresStoreResolution)
        {
            var lookup = await geoLookupService.ResolveAsync(
                new GeoLookupRequest(city, area, request.Lat, request.Lng),
                cancellationToken);

            address.ServingStoreId = lookup.ServingStoreId;
            address.IsServiceable = lookup.IsServiceable;
        }

        await addressRepository.Update(address);
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Success<object>(
            ToResponse(address),
            "Address updated successfully.",
            "Address updated successfully.");
    }

    private static AddressResponse ToResponse(UserAddress address) => new(
        address.Id,
        address.RecipientName,
        address.Phone,
        address.AddressLine,
        address.City,
        address.Area,
        address.Lat,
        address.Lng,
        address.Label,
        address.ServingStoreId,
        address.IsServiceable,
        address.IsDefault,
        address.CreatedAtUtc);
}
