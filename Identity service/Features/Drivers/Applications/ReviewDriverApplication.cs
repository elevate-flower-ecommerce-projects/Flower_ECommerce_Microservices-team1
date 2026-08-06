using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Errors;
using Identity_service.Persistence;
using MediatR;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications;

public sealed record ApproveDriverApplicationCommand(Guid ApplicationId, string ReviewedBy)
    : IRequest<Result<DriverApplicationStatusResponse>>;

public sealed record RejectDriverApplicationCommand(Guid ApplicationId, string ReviewedBy, string RejectionReason)
    : IRequest<Result<DriverApplicationStatusResponse>>;

public sealed class ApproveDriverApplicationHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<ApproveDriverApplicationCommand, Result<DriverApplicationStatusResponse>>
{
    public async Task<Result<DriverApplicationStatusResponse>> Handle(
        ApproveDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<DriverApplication, Guid>().Get(request.ApplicationId);
        if (application is null)
            return Result.Failure<DriverApplicationStatusResponse>(DriverApplicationErrors.NotFound);

        if (application.Status != DriverApplicationStatus.PendingReview)
            return Result.Failure<DriverApplicationStatusResponse>(DriverApplicationErrors.InvalidTransition);

        application.Status = DriverApplicationStatus.Approved;
        application.ReviewedBy = request.ReviewedBy;
        application.ReviewedAt = DateTime.UtcNow;
        application.RejectionReason = null;

        await unitOfWork.Repository<DriverApplication, Guid>().Update(application);
        await unitOfWork.CompleteAsync();

        return Result.Success(ToResponse(application));
    }

    private static DriverApplicationStatusResponse ToResponse(DriverApplication application)
        => new(application.Id, application.Status, application.RejectionReason, application.SubmittedAt, application.ReviewedAt);
}

public sealed class RejectDriverApplicationHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<RejectDriverApplicationCommand, Result<DriverApplicationStatusResponse>>
{
    public async Task<Result<DriverApplicationStatusResponse>> Handle(
        RejectDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<DriverApplication, Guid>().Get(request.ApplicationId);
        if (application is null)
            return Result.Failure<DriverApplicationStatusResponse>(DriverApplicationErrors.NotFound);

        if (application.Status != DriverApplicationStatus.PendingReview)
            return Result.Failure<DriverApplicationStatusResponse>(DriverApplicationErrors.InvalidTransition);

        application.Status = DriverApplicationStatus.Rejected;
        application.ReviewedBy = request.ReviewedBy;
        application.ReviewedAt = DateTime.UtcNow;
        application.RejectionReason = request.RejectionReason.Trim();

        await unitOfWork.Repository<DriverApplication, Guid>().Update(application);
        await unitOfWork.CompleteAsync();

        return Result.Success(ToResponse(application));
    }

    private static DriverApplicationStatusResponse ToResponse(DriverApplication application)
        => new(application.Id, application.Status, application.RejectionReason, application.SubmittedAt, application.ReviewedAt);
}
