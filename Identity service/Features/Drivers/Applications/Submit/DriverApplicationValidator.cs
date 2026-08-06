using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Identity_service.Entities;
using Identity_service.Persistence;
using Identity_service.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications.Submit;

/// <summary>
/// Validates driver application input before the handler creates any records.
/// </summary>
public sealed partial class DriverApplicationValidator(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork<ApplicationDbContext> unitOfWork,
    IOptions<DriverDocumentStorageOptions> documentOptions) : IDriverApplicationValidator
{
    public async Task<Dictionary<string, string[]>> ValidateAsync(
        SubmitDriverApplicationCommand request,
        CancellationToken cancellationToken)
    {
        #region Required fields and format validation

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        var nationalId = request.NationalId.Trim();

        AddIf(errors, nameof(request.FullName), string.IsNullOrWhiteSpace(request.FullName), "Full name is required.");
        AddIf(errors, nameof(request.Phone), string.IsNullOrWhiteSpace(phone), "Phone is required.");
        AddIf(errors, nameof(request.Email), !new EmailAddressAttribute().IsValid(request.Email), "A valid email is required.");
        AddIf(errors, nameof(request.NationalId), string.IsNullOrWhiteSpace(nationalId), "National ID number is required.");
        AddIf(errors, nameof(request.VehiclePlateNumber), string.IsNullOrWhiteSpace(request.VehiclePlateNumber), "Vehicle plate number is required.");
        AddIf(errors, nameof(request.Password), !PasswordRegex().IsMatch(request.Password), "Password must be at least 6 characters and contain 1 uppercase letter and 1 digit.");
        AddIf(errors, nameof(request.ConfirmPassword), request.Password != request.ConfirmPassword, "Confirm password must match password.");
        AddIf(errors, nameof(request.Documents), request.Documents.Count == 0, "At least one identity or license document is required.");

        ValidateDocuments(request, errors);

        #endregion

        // Stop before database checks when the request is already structurally invalid.
        if (errors.Count > 0)
            return errors;

        #region Duplicate checks

        if (await userManager.FindByEmailAsync(email) is not null)
            errors[nameof(request.Email)] = ["Email is already registered."];

        if (await unitOfWork.Repository<ApplicationUser, string>().Query()
            .AnyAsync(user => user.PhoneNumber == phone, cancellationToken))
        {
            errors[nameof(request.Phone)] = ["Phone is already registered."];
        }

        if (await unitOfWork.Repository<DriverProfile, Guid>().Query()
            .AnyAsync(profile => profile.NationalId == nationalId, cancellationToken))
        {
            errors[nameof(request.NationalId)] = ["National ID number is already registered."];
        }

        return errors;

        #endregion
    }

    #region Document validation

    private void ValidateDocuments(
        SubmitDriverApplicationCommand request,
        Dictionary<string, string[]> errors)
    {
        var options = documentOptions.Value;
        var allowedContentTypes = options.AllowedContentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var documentErrors = new List<string>();

        foreach (var file in request.Documents)
        {
            if (file.Length <= 0)
                documentErrors.Add($"{file.FileName} is empty.");

            if (file.Length > options.MaxFileSizeBytes)
                documentErrors.Add($"{file.FileName} exceeds the 5MB size limit.");

            if (!allowedContentTypes.Contains(file.ContentType) || !AllowedExtensionRegex().IsMatch(file.FileName))
                documentErrors.Add($"{file.FileName} must be a jpg, png, or pdf file.");
        }

        if (documentErrors.Count > 0)
            errors[nameof(request.Documents)] = [.. documentErrors];
    }

    #endregion

    #region Helpers

    private static void AddIf(
        Dictionary<string, string[]> errors,
        string field,
        bool condition,
        string message)
    {
        if (condition)
            errors[field] = [message];
    }

    [GeneratedRegex(@"^(?=.*[A-Z])(?=.*\d).{6,}$")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"\.(jpe?g|png|pdf)$", RegexOptions.IgnoreCase)]
    private static partial Regex AllowedExtensionRegex();

    #endregion
}
