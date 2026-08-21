using Address___Store_Coverage_Service.Entities;
using Address___Store_Coverage_Service.Persistence;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Address___Store_Coverage_Service.Features.Addresses.Delete;

public sealed class DeleteAddressHandler(
    IUnitOfWork<AddressDbContext> unitOfWork) : IRequestHandler<DeleteAddressCommand, OperationResult>
{
    public async Task<OperationResult> Handle(
        DeleteAddressCommand request,
        CancellationToken cancellationToken)
    {
        var addressRepository = unitOfWork.Repository<UserAddress, Guid>();
        var address = await addressRepository
            .Query()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.AddressId && candidate.UserId == request.UserId,
                cancellationToken);

        if (address is null)
        {
            return OperationResultFactory.NotFound(
                message: "Address was not found.",
                messageLocalized: "Address was not found.");
        }

        var deleteSucceeded = false;
        var affectedRows = 0;

        await addressRepository.ExecuteInTransactionAsync(async () =>
        {
            if (address.IsDefault)
            {
                var replacement = await addressRepository
                    .Query()
                    .Where(candidate => candidate.UserId == request.UserId && candidate.Id != address.Id)
                    .OrderByDescending(candidate => candidate.CreatedAtUtc)
                    .FirstOrDefaultAsync(cancellationToken);

                if (replacement is not null)
                {
                    replacement.IsDefault = true;
                    await addressRepository.Update(replacement);
                }
            }

            deleteSucceeded = await addressRepository.Delete(address);
            if (!deleteSucceeded)
                return;

            affectedRows = await unitOfWork.CompleteAsync();
        });

        if (!deleteSucceeded || affectedRows <= 0)
        {
            return OperationResultFactory.Error(
                message: "Address deletion could not be persisted.",
                messageLocalized: "Address deletion could not be persisted.");
        }

        return OperationResultFactory.NoContent(
            "Address deleted successfully.",
            "Address deleted successfully.");
    }
}
