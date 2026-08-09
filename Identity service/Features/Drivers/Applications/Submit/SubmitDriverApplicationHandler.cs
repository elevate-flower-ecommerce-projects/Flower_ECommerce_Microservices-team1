using Flower.Common.StandardizedResponse;
using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications.Submit;

/// <summary>
/// Orchestrates the complete prospective-driver application workflow.
/// </summary>
public sealed class SubmitDriverApplicationHandler(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    IDriverDocumentStorage documentStorage,
    IDriverApplicationValidator validator) : IRequestHandler<SubmitDriverApplicationCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        SubmitDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        #region Validate applicant data

        var errors = await validator.ValidateAsync(request, cancellationToken);
        if (errors.Count > 0)
            return OperationResultFactory.Validation<object>(
                errors,
                "Driver application validation failed.",
                "Driver application validation failed.");

        #endregion

        #region Create identity account

        var email = request.Email.Trim().ToLowerInvariant();
        var name = SplitFullName(request.FullName);
        var user = new ApplicationUser
        {
            FirstName = name.FirstName,
            LastName = name.LastName,
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim()
        };

        var created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var identityErrors = ToIdentityErrors(created);
            var message = string.Join(" ", identityErrors.SelectMany(error => error.Value));
            return OperationResultFactory.Validation<object>(identityErrors, message, message);
        }

        var addedToRole = await userManager.AddToRoleAsync(user, ApplicationRoleNames.Driver);
        if (!addedToRole.Succeeded)
        {
            var identityErrors = ToIdentityErrors(addedToRole);
            var message = string.Join(" ", identityErrors.SelectMany(error => error.Value));
            return OperationResultFactory.Validation<object>(identityErrors, message, message);
        }

        #endregion

        #region Create driver profile and application

        var applicationId = Guid.CreateVersion7();
        await unitOfWork.Repository<DriverProfile, Guid>().Create(new DriverProfile
        {
            UserId = user.Id,
            NationalId = request.NationalId.Trim(),
            VehicleType = request.VehicleType,
            PlateNumber = request.VehiclePlateNumber.Trim()
        });

        await unitOfWork.Repository<DriverApplication, Guid>().Create(new DriverApplication
        {
            Id = applicationId,
            UserId = user.Id,
            Status = DriverApplicationStatus.PendingReview,
            SubmittedAt = DateTime.UtcNow
        });

        #endregion

        #region Store private documents and metadata

        foreach (var file in request.Documents)
        {
            var storedDocument = await documentStorage.SaveAsync(applicationId, file, cancellationToken);
            await unitOfWork.Repository<DriverDocument, Guid>().Create(new DriverDocument
            {
                ApplicationId = applicationId,
                FileUrl = storedDocument.StorageKey,
                DocType = ResolveDocumentType(file),
                OriginalFileName = storedDocument.OriginalFileName,
                ContentType = storedDocument.ContentType,
                SizeInBytes = storedDocument.SizeInBytes
            });
        }

        #endregion

        #region Persist application workflow

        await unitOfWork.CompleteAsync();

        return OperationResultFactory.Created<object>(
            new SubmitDriverApplicationResponse(applicationId, DriverApplicationStatus.PendingReview),
            "Driver application submitted successfully.",
            "Driver application submitted successfully.");

        #endregion
    }

    #region Helpers

    private static Dictionary<string, string[]> ToIdentityErrors(IdentityResult result)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var error in result.Errors)
        {
            var field = error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? "Password"
                : "Email";

            errors[field] = errors.TryGetValue(field, out var existing)
                ? [.. existing, error.Description]
                : [error.Description];
        }

        return errors;
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? (parts[0], parts[0])
            : (parts[0], parts[1]);
    }

    private static string ResolveDocumentType(IFormFile file)
        => file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            ? "LicensePdf"
            : "IdentityImage";

    #endregion
}
