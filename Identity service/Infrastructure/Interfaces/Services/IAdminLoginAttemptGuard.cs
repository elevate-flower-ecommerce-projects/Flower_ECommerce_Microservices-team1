namespace Identity_service.Infrastructure.Interfaces.Services;

public interface IAdminLoginAttemptGuard
{
    bool IsBlocked(string email, string? ipAddress);
    void RegisterFailure(string email, string? ipAddress);
    void ResetAccount(string email);
}
