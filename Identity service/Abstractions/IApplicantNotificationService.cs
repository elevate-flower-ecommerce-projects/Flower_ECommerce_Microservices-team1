using Identity_service.Entities;

namespace Identity_service.Abstractions;

public interface IApplicantNotificationService
{
    Task NotifyDriverApplicationDecisionAsync(
        ApplicationUser applicant,
        DriverApplication application,
        CancellationToken cancellationToken);
}
