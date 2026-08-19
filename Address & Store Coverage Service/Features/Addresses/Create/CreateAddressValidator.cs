using System.Text.RegularExpressions;

namespace Address___Store_Coverage_Service.Features.Addresses.Create;

public sealed partial class CreateAddressValidator : ICreateAddressValidator
{
    public Dictionary<string, string[]> Validate(CreateAddressCommand request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddRequired(errors, nameof(request.RecipientName), request.RecipientName, "Recipient name is required.");
        AddRequired(errors, nameof(request.Phone), request.Phone, "Phone is required.");
        AddRequired(errors, nameof(request.AddressLine), request.AddressLine, "Address line is required.");
        AddRequired(errors, nameof(request.City), request.City, "City is required.");
        AddRequired(errors, nameof(request.Area), request.Area, "Area is required.");

        var phone = request.Phone?.Trim() ?? string.Empty;
        if (phone.Length > 0 && !EgyptianMobileRegex().IsMatch(phone))
        {
            AddIf(errors, nameof(request.Phone), true, "Enter a valid Egyptian mobile number (01[0-2,5]XXXXXXXX).");
        }

        AddIf(errors, nameof(request.RecipientName), request.RecipientName?.Trim().Length > 120, "Recipient name must not exceed 120 characters.");
        AddIf(errors, nameof(request.AddressLine), request.AddressLine?.Trim().Length > 500, "Address line must not exceed 500 characters.");
        AddIf(errors, nameof(request.City), request.City?.Trim().Length > 120, "City must not exceed 120 characters.");
        AddIf(errors, nameof(request.Area), request.Area?.Trim().Length > 120, "Area must not exceed 120 characters.");
        AddIf(errors, nameof(request.Label), request.Label?.Trim().Length > 50, "Label must not exceed 50 characters.");
        AddIf(errors, nameof(request.Lat), request.Lat is < -90 or > 90, "Latitude must be between -90 and 90.");
        AddIf(errors, nameof(request.Lng), request.Lng is < -180 or > 180, "Longitude must be between -180 and 180.");

        if ((request.Lat is null) != (request.Lng is null))
        {
            AddIf(errors, request.Lat is null ? nameof(request.Lat) : nameof(request.Lng), true, "Latitude and longitude must be supplied together.");
        }

        return errors;
    }

    private static void AddRequired(Dictionary<string, string[]> errors, string field, string? value, string message)
        => AddIf(errors, field, string.IsNullOrWhiteSpace(value), message);

    private static void AddIf(Dictionary<string, string[]> errors, string field, bool condition, string message)
    {
        if (!condition)
            return;

        errors[field] = errors.TryGetValue(field, out var current)
            ? [.. current, message]
            : [message];
    }

    [GeneratedRegex(@"^01[0125]\d{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex EgyptianMobileRegex();
}

public interface ICreateAddressValidator
{
    Dictionary<string, string[]> Validate(CreateAddressCommand request);
}
