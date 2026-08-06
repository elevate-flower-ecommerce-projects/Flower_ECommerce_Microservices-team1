using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity_service.Features.Users.Register;

/// <summary>
/// Creates the Identity account, customer profile, and role assignment atomically.
/// </summary>
public sealed class RegisterCustomerHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IRegisterCustomerValidator validator,
    ILogger<RegisterCustomerHandler> logger)
    : IRequestHandler<RegisterCustomerCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var validationErrors = await validator.ValidateAsync(request, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return OperationResultFactory.Validation<object>(
                validationErrors,
                "Customer registration validation failed.",
                "Customer registration validation failed.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber.Trim();
        var name = SplitFullName(request.FullName);
        _ = Enum.TryParse<Gender>(request.Gender.Trim(), ignoreCase: true, out var gender);

        try
        {
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var user = new ApplicationUser
                {
                    FirstName = name.FirstName,
                    LastName = name.LastName,
                    UserName = email,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Gender = gender,
                    IsDisabled = false
                };

                // UserManager hashes the password before the user is persisted.
                var createUserResult = await userManager.CreateAsync(user, request.Password);
                if (!createUserResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    var identityErrors = MapIdentityErrors(createUserResult);

                    return OperationResultFactory.Validation<object>(
                        identityErrors,
                        "Customer registration validation failed.",
                        "Customer registration validation failed.");
                }

                dbContext.CustomerProfiles.Add(new CustomerProfile
                {
                    UserId = user.Id
                });

                await dbContext.SaveChangesAsync(cancellationToken);

                var addToRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoleNames.Customer);
                if (!addToRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError(
                        "Failed to assign the Customer role to user {UserId}. Identity errors: {ErrorCodes}",
                        user.Id,
                        string.Join(", ", addToRoleResult.Errors.Select(error => error.Code)));

                    return RegistrationFailure();
                }

                await transaction.CommitAsync(cancellationToken);

                return OperationResultFactory.Created<object>(
                    new RegisterCustomerResponse(
                        user.Id,
                        email,
                        ApplicationRoleNames.Customer,
                        "Active"),
                    "Account registered successfully.",
                    "Account registered successfully.");
            });
        }
        catch (DbUpdateException exception) when (TryMapDuplicateError(exception, out var duplicateErrors))
        {
            logger.LogWarning(
                "A duplicate email or phone number was detected while registering {Email}.",
                email);

            return OperationResultFactory.Validation<object>(
                duplicateErrors,
                "Customer registration validation failed.",
                "Customer registration validation failed.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Customer registration failed for {Email}.", email);
            return RegistrationFailure();
        }
    }

    private static OperationResult<object> RegistrationFailure()
        => OperationResultFactory.Error<object>(
            message: "Unable to register the account at this time.",
            messageLocalized: "Unable to register the account at this time.");

    private static Dictionary<string, string[]> MapIdentityErrors(IdentityResult result)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var error in result.Errors)
        {
            var field = ResolveIdentityErrorField(error.Code);
            var message = field == nameof(RegisterCustomerCommand.Email)
                && error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
                    ? RegisterCustomerMessages.EmailAlreadyRegistered
                    : error.Description;

            errors[field] = errors.TryGetValue(field, out var current)
                ? [.. current, message]
                : [message];
        }

        return errors;
    }

    private static string ResolveIdentityErrorField(string errorCode)
    {
        if (errorCode.Contains("Password", StringComparison.OrdinalIgnoreCase))
            return nameof(RegisterCustomerCommand.Password);

        if (errorCode.Contains("Email", StringComparison.OrdinalIgnoreCase)
            || errorCode.Contains("UserName", StringComparison.OrdinalIgnoreCase))
        {
            return nameof(RegisterCustomerCommand.Email);
        }

        return "Registration";
    }

    private static bool TryMapDuplicateError(
        DbUpdateException exception,
        out Dictionary<string, string[]> errors)
    {
        var databaseMessage = exception.InnerException?.Message ?? exception.Message;

        if (databaseMessage.Contains("UX_ApplicationUser_PhoneNumber", StringComparison.OrdinalIgnoreCase))
        {
            errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(RegisterCustomerCommand.PhoneNumber)] =
                    [RegisterCustomerMessages.PhoneNumberAlreadyRegistered]
            };
            return true;
        }

        if (databaseMessage.Contains("UserNameIndex", StringComparison.OrdinalIgnoreCase)
            || databaseMessage.Contains("NormalizedUserName", StringComparison.OrdinalIgnoreCase))
        {
            errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(RegisterCustomerCommand.Email)] =
                    [RegisterCustomerMessages.EmailAlreadyRegistered]
            };
            return true;
        }

        errors = [];
        return false;
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? (parts[0], parts[0])
            : (parts[0], parts[1]);
    }
}
