namespace Identity_service.Features.Users.UpdateProfile;

/// <summary>
/// Mirrors the Edit profile screen. Driver callers may include vehicle details in the same request.
/// </summary>
public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Nullable so a missing value is reported as required instead of defaulting.</summary>
    public Gender? Gender { get; set; }

    /// <summary>Optional. Omitting it keeps the current avatar.</summary>
    public IFormFile? ProfilePicture { get; set; }

    /// <summary>Driver only. Omit to keep the current vehicle type.</summary>
    public VehicleType? VehicleType { get; set; }

    /// <summary>Driver only. Omit to keep the current vehicle plate number.</summary>
    public string? VehiclePlateNumber { get; set; }

    /// <summary>Driver only. Omit to keep the current country.</summary>
    public string? Country { get; set; }
}