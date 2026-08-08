using Flower.Common.StandardizedResponse;
using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications;

public sealed record ApproveDriverApplicationCommand(Guid ApplicationId, string ReviewedBy)
    : IRequest<OperationResult<object>>;

public sealed record RejectDriverApplicationCommand(Guid ApplicationId, string ReviewedBy, string RejectionReason)
    : IRequest<OperationResult<object>>;

public sealed record ReviewDriverApplicationResponse(
    Guid ApplicationId,
    DriverApplicationStatus Status,
    string? RejectionReason,
    string? ReviewedBy,
    DateTime? ReviewedAt,
    DateTime SubmittedAt);

public sealed class ApproveDriverApplicationHandler(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    IApplicantNotificationService notifications)
    : IRequestHandler<ApproveDriverApplicationCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        ApproveDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<DriverApplication, Guid>()
            .Query(false, driverApplication => driverApplication.User!)
            .SingleOrDefaultAsync(driverApplication => driverApplication.Id == request.ApplicationId, cancellationToken);

        if (application is null)
            return OperationResultFactory.NotFound<object>(message: "Driver application was not found.");

        if (application.Status != DriverApplicationStatus.PendingReview)
            return OperationResultFactory.Conflict<object>(
                ToResponse(application),
                "Only pending applications can be reviewed.");

        application.Status = DriverApplicationStatus.Approved;
        application.ReviewedBy = request.ReviewedBy;
        application.ReviewedAt = DateTime.UtcNow;
        application.RejectionReason = null;

        await unitOfWork.Repository<DriverApplication, Guid>().Update(application);
        await unitOfWork.CompleteAsync();

        if (application.User is not null)
            await notifications.NotifyDriverApplicationDecisionAsync(application.User, application, cancellationToken);

        return OperationResultFactory.Success<object>(
            ToResponse(application),
            "Driver application approved successfully.");
    }

    private static ReviewDriverApplicationResponse ToResponse(DriverApplication application)
        => new(application.Id, application.Status, application.RejectionReason, application.ReviewedBy, application.ReviewedAt, application.SubmittedAt);
}

public sealed class RejectDriverApplicationHandler(
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    IApplicantNotificationService notifications)
    : IRequestHandler<RejectDriverApplicationCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        RejectDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return OperationResultFactory.Validation<object>(
                new Dictionary<string, string[]>
                {
                    [nameof(request.RejectionReason)] = ["Rejection reason is required."]
                });

        var application = await unitOfWork.Repository<DriverApplication, Guid>()
            .Query(false, driverApplication => driverApplication.User!)
            .SingleOrDefaultAsync(driverApplication => driverApplication.Id == request.ApplicationId, cancellationToken);

        if (application is null)
            return OperationResultFactory.NotFound<object>(message: "Driver application was not found.");

        if (application.Status != DriverApplicationStatus.PendingReview)
            return OperationResultFactory.Conflict<object>(
                ToResponse(application),
                "Only pending applications can be reviewed.");

        application.Status = DriverApplicationStatus.Rejected;
        application.ReviewedBy = request.ReviewedBy;
        application.ReviewedAt = DateTime.UtcNow;
        application.RejectionReason = request.RejectionReason.Trim();

        await unitOfWork.Repository<DriverApplication, Guid>().Update(application);
        await unitOfWork.CompleteAsync();

        if (application.User is not null)
            await notifications.NotifyDriverApplicationDecisionAsync(application.User, application, cancellationToken);

        return OperationResultFactory.Success<object>(
            ToResponse(application),
            "Driver application rejected successfully.");
    }

    private static ReviewDriverApplicationResponse ToResponse(DriverApplication application)
        => new(application.Id, application.Status, application.RejectionReason, application.ReviewedBy, application.ReviewedAt, application.SubmittedAt);
}
