using System.Text.RegularExpressions;
using Order___Fulfillment_Service.Contracts.Checkout;
using Order___Fulfillment_Service.Entities;

namespace Order___Fulfillment_Service.Features.Checkout;

public static partial class CheckoutValidator
{
    public const int IdempotencyKeyMaxLength = 100;

    /// <summary>Where the order goes. Gift fields follow the Address service's rules for a saved address.</summary>
    public static Dictionary<string, string[]> ValidateDelivery(Guid? addressId, GiftDetailsRequest? gift)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddIf(errors, "AddressId", addressId is not null && gift is not null, "Send either addressId or gift, not both.");
        AddIf(errors, "AddressId", addressId == Guid.Empty, "Address id is not valid.");

        if (gift is not null)
        {
            AddRequired(errors, "Gift.RecipientName", gift.RecipientName, "Recipient name is required.");
            AddRequired(errors, "Gift.Phone", gift.Phone, "Phone is required.");
            AddRequired(errors, "Gift.AddressLine", gift.AddressLine, "Address line is required.");
            AddRequired(errors, "Gift.City", gift.City, "City is required.");
            AddRequired(errors, "Gift.Area", gift.Area, "Area is required.");

            var phone = gift.Phone?.Trim() ?? string.Empty;
            AddIf(errors, "Gift.Phone", phone.Length > 0 && !EgyptianMobileRegex().IsMatch(phone), "Enter a valid Egyptian mobile number (01[0-2,5]XXXXXXXX).");

            AddIf(errors, "Gift.RecipientName", gift.RecipientName?.Trim().Length > 120, "Recipient name must not exceed 120 characters.");
            AddIf(errors, "Gift.AddressLine", gift.AddressLine?.Trim().Length > 500, "Address line must not exceed 500 characters.");
            AddIf(errors, "Gift.City", gift.City?.Trim().Length > 120, "City must not exceed 120 characters.");
            AddIf(errors, "Gift.Area", gift.Area?.Trim().Length > 120, "Area must not exceed 120 characters.");
            AddIf(errors, "Gift.Message", gift.Message?.Trim().Length > 500, "Gift message must not exceed 500 characters.");
            AddIf(errors, "Gift.Lat", gift.Lat is < -90 or > 90, "Latitude must be between -90 and 90.");
            AddIf(errors, "Gift.Lng", gift.Lng is < -180 or > 180, "Longitude must be between -180 and 180.");
            AddIf(errors, gift.Lat is null ? "Gift.Lat" : "Gift.Lng", (gift.Lat is null) != (gift.Lng is null), "Latitude and longitude must be supplied together.");
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidatePlaceOrder(PlaceOrderRequest request)
    {
        var errors = ValidateDelivery(request.AddressId, request.Gift);

        AddIf(errors, "PaymentMethod", request.PaymentMethod is null || !Enum.IsDefined(request.PaymentMethod.Value), "Choose Cash on Delivery (1) or Card (2).");
        AddIf(errors, "PaymentMethod", request.PaymentMethod is PaymentMethodType.Wallet, "Wallet payments are not available yet.");
        AddIf(errors, "ExpectedTotal", request.ExpectedTotal is null, "Send the total shown in the checkout summary.");
        AddIf(errors, "ExpectedTotal", request.ExpectedTotal < 0, "The expected total cannot be negative.");

        return errors;
    }

    public static bool IsValidIdempotencyKey(string? key)
        => !string.IsNullOrWhiteSpace(key) && key.Trim().Length <= IdempotencyKeyMaxLength;

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
