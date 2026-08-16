namespace Identity_service.Features.Users.Register;

public interface IRegisterCustomerValidator
{
    Task<Dictionary<string, string[]>> ValidateAsync(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken);
}
