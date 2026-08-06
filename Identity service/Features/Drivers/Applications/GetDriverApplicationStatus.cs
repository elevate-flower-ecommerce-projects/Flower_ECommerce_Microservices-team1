using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Errors;
using Identity_service.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications;

public sealed record GetDriverApplicationStatusQuery(string UserId)
    : IRequest<Result<DriverApplicationStatusResponse>>;

public sealed record DriverApplicationStatusResponse(
    Guid ApplicationId,
    DriverApplicationStatus Status,
    string? RejectionReason,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);

public sealed class GetDriverApplicationStatusHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<GetDriverApplicationStatusQuery, Result<DriverApplicationStatusResponse>>
{
    public async Task<Result<DriverApplicationStatusResponse>> Handle(
        GetDriverApplicationStatusQuery request,
        CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<DriverApplication, Guid>()
            .Query()
            .Where(driverApplication => driverApplication.UserId == request.UserId)
            .Select(driverApplication => new DriverApplicationStatusResponse(
                driverApplication.Id,
                driverApplication.Status,
                driverApplication.RejectionReason,
                driverApplication.SubmittedAt,
                driverApplication.ReviewedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return application is null
            ? Result.Failure<DriverApplicationStatusResponse>(DriverApplicationErrors.NotFound)
            : Result.Success(application);
    }
}
