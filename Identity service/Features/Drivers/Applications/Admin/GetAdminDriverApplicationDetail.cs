using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications.Admin;

public sealed record GetAdminDriverApplicationDetailQuery(Guid ApplicationId)
    : IRequest<OperationResult<object>>;

public sealed class GetAdminDriverApplicationDetailHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<GetAdminDriverApplicationDetailQuery, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        GetAdminDriverApplicationDetailQuery request,
        CancellationToken cancellationToken)
    {
        #region Project full application details

        var application = await unitOfWork.Repository<DriverApplication, Guid>()
            .Query()
            .Where(driverApplication => driverApplication.Id == request.ApplicationId)
            .Select(driverApplication => new AdminDriverApplicationDetailResponse(
                driverApplication.Id,
                driverApplication.Status,
                $"{driverApplication.User!.FirstName} {driverApplication.User.LastName}",
                driverApplication.User.Email!,
                driverApplication.User.PhoneNumber,
                driverApplication.User.DriverProfile!.NationalId,
                driverApplication.User.DriverProfile.VehicleType,
                driverApplication.User.DriverProfile.PlateNumber,
                driverApplication.RejectionReason,
                driverApplication.ReviewedBy,
                driverApplication.ReviewedAt,
                driverApplication.SubmittedAt,
                driverApplication.Documents
                    .OrderBy(document => document.UploadedAt)
                    .Select(document => new AdminDriverDocumentResponse(
                        document.Id,
                        document.DocType,
                        document.OriginalFileName,
                        document.ContentType,
                        document.SizeInBytes,
                        document.UploadedAt,
                        $"/admin/drivers/applications/{driverApplication.Id}/documents/{document.Id}/download"))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        #endregion

        return application is null
            ? OperationResultFactory.NotFound<object>(message: "Driver application was not found.")
            : OperationResultFactory.Success<object>(application);
    }
}
