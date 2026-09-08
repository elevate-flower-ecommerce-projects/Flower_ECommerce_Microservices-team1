namespace Identity_service.Features.Users.StoreFCMToken;

public class UpSertFCMTokenOrchestratorValidator : AbstractValidator<UpSertFCMTokenOrchestrator>
{
    public UpSertFCMTokenOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId is required.");
        RuleFor(x => x.FCMToken)
            .NotEmpty().WithMessage("FCMToken is required.");
    }
}