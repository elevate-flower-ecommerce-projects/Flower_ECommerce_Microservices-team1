namespace Payment_Service.Infrastructure.Stripe;

/// <summary>
/// Stripe takes amounts in the currency's smallest unit, so 1600.00 EGP is sent as 160000 piastres.
/// A handful of currencies have no minor unit at all and are sent as whole units instead; getting
/// that wrong would charge a hundred times too much, so they are listed explicitly.
/// </summary>
public static class MoneyConversion
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga",
        "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    public static bool TryToMinorUnits(decimal amount, string currency, out long minorUnits)
    {
        minorUnits = 0;
        if (amount <= 0m)
            return false;

        var scaled = ZeroDecimalCurrencies.Contains(currency)
            ? decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
            : decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

        if (scaled is < 1m or > long.MaxValue)
            return false;

        minorUnits = (long)scaled;
        return true;
    }

    public static decimal FromMinorUnits(long minorUnits, string currency)
        => ZeroDecimalCurrencies.Contains(currency) ? minorUnits : minorUnits / 100m;
}
