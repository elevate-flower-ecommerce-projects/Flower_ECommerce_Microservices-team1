namespace Identity_service.Features.Drivers.Applications.Submit;

public interface IDriverApplicationValidator
{
    Task<Dictionary<string, string[]>> ValidateAsync(
        SubmitDriverApplicationCommand request,
        CancellationToken cancellationToken);
}
