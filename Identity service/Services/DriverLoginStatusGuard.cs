using Identity_service.Entities;
using Identity_service.Persistence;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Services;

public sealed record DriverLoginStatusDecision(
    bool CanAccessDriverHome,
    DriverApplicationStatus? Status,
    string? RejectionReason);

/// <summary>
/// Central auth-layer guard for blocking driver access until the application is approved.
/// </summary>
public interface IDriverLoginStatusGuard
{
    Task<DriverLoginStatusDecision> CheckAsync(string userId, CancellationToken cancellationToken);
}

public sealed class DriverLoginStatusGuard(IUnitOfWork<ApplicationDbContext> unitOfWork) : IDriverLoginStatusGuard
{
    #region Status check

    public async Task<DriverLoginStatusDecision> CheckAsync(string userId, CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<DriverApplication, Guid>()
            .Query()
            .Where(driverApplication => driverApplication.UserId == userId)
            .Select(driverApplication => new
            {
                driverApplication.Status,
                driverApplication.RejectionReason
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null || application.Status == DriverApplicationStatus.Approved)
            return new DriverLoginStatusDecision(true, application?.Status, null);

        return new DriverLoginStatusDecision(false, application.Status, application.RejectionReason);
    }

    #endregion
}
