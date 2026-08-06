using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Identity_service.Features.Users.Register;

/// <summary>
/// Anonymous minimal API endpoint for customer account registration.
/// </summary>
public sealed class RegisterCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users");

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterCustomer")
            .WithSummary("Register a customer account")
            .Accepts<RegisterCustomerRequest>("application/json")
            .Produces<OperationResult<RegisterCustomerResponse>>(StatusCodes.Status201Created)
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCustomerCommand(
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.Gender,
            request.Password,
            request.ConfirmPassword), cancellationToken);

        return result.ToHttpResult();
    }
}
