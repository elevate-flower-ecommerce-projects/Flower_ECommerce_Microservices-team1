namespace Order___Fulfillment_Service.Settings;

public sealed class CheckoutOptions
{
    public const string SectionName = "Checkout";
    public decimal DeliveryFee { get; set; } = 50m;
}
