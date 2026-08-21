using Address___Store_Coverage_Service.Features.Addresses;
using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Address___Store_Coverage_Service.Services.GeoLookup;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.Create;

public sealed class CreateAddressHandler(
    IUnitOfWork<AddressDbContext> unitOfWork,
    IGeoLookupService geoLookupService,
    ICreateAddressValidator validator) : IRequestHandler<CreateAddressCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        CreateAddressCommand request,
        CancellationToken cancellationToken)
    {
        var errors = validator.Validate(request);
        if (errors.Count > 0)
        {
            return OperationResultFactory.Validation<object>(
                errors,
                "Address validation failed.",
                "Address validation failed.");
        }

        var addressRepository = unitOfWork.Repository<UserAddress, Guid>();
        var isFirstAddress = !await addressRepository
            .Query()
            .AnyAsync(address => address.UserId == request.UserId, cancellationToken);

        var lookup = await geoLookupService.ResolveAsync(
            new GeoLookupRequest(request.City, request.Area, request.Lat, request.Lng),
            cancellationToken);

        var address = new UserAddress
        {
            Id = Guid.CreateVersion7(),
            UserId = request.UserId,
            RecipientName = request.RecipientName.Trim(),
            Phone = request.Phone.Trim(),
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            Area = request.Area.Trim(),
            Lat = request.Lat,
            Lng = request.Lng,
            Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim(),
            ServingStoreId = lookup.ServingStoreId,
            IsServiceable = lookup.IsServiceable,
            IsDefault = isFirstAddress,
            CreatedAtUtc = DateTime.UtcNow
        };

        await addressRepository.Create(address);
        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Created<object>(
            AddressMapping.ToResponse(address),
            "Address created successfully.",
            "Address created successfully.");
    }
}
