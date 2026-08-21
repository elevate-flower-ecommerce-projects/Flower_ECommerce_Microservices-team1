using Address___Store_Coverage_Service.Contracts.Stores;

namespace Address___Store_Coverage_Service.Features.Stores.Admin;

public static class StoreRequestValidator
{
    public static Dictionary<string, string[]> Validate(StoreRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddRequired(errors, nameof(request.Name), request.Name, "Store name is required.");
        AddRequired(errors, nameof(request.Location), request.Location, "Store location is required.");
        AddIf(errors, nameof(request.Name), request.Name?.Trim().Length > 160, "Store name must not exceed 160 characters.");
        AddIf(errors, nameof(request.Location), request.Location?.Trim().Length > 500, "Store location must not exceed 500 characters.");
        AddIf(errors, nameof(request.Lat), request.Lat is < -90 or > 90, "Latitude must be between -90 and 90.");
        AddIf(errors, nameof(request.Lng), request.Lng is < -180 or > 180, "Longitude must be between -180 and 180.");
        AddIf(errors, nameof(request.Lat), (request.Lat is null) != (request.Lng is null), "Store latitude and longitude must be supplied together.");

        var coverageAreas = request.CoverageAreas ?? [];
        for (var i = 0; i < coverageAreas.Count; i++)
        {
            var coverage = coverageAreas[i];
            var prefix = $"coverageAreas[{i}]";
            AddRequired(errors, $"{prefix}.city", coverage.City, "Coverage city is required.");
            AddRequired(errors, $"{prefix}.area", coverage.Area, "Coverage area is required.");
            AddIf(errors, $"{prefix}.city", coverage.City?.Trim().Length > 120, "Coverage city must not exceed 120 characters.");
            AddIf(errors, $"{prefix}.area", coverage.Area?.Trim().Length > 120, "Coverage area must not exceed 120 characters.");
            AddIf(errors, $"{prefix}.minLat", coverage.MinLat is < -90 or > 90, "Minimum latitude must be between -90 and 90.");
            AddIf(errors, $"{prefix}.maxLat", coverage.MaxLat is < -90 or > 90, "Maximum latitude must be between -90 and 90.");
            AddIf(errors, $"{prefix}.minLng", coverage.MinLng is < -180 or > 180, "Minimum longitude must be between -180 and 180.");
            AddIf(errors, $"{prefix}.maxLng", coverage.MaxLng is < -180 or > 180, "Maximum longitude must be between -180 and 180.");

            var hasAnyBoundary = coverage.MinLat is not null || coverage.MaxLat is not null || coverage.MinLng is not null || coverage.MaxLng is not null;
            var hasFullBoundary = coverage.MinLat is not null && coverage.MaxLat is not null && coverage.MinLng is not null && coverage.MaxLng is not null;
            AddIf(errors, $"{prefix}.boundary", hasAnyBoundary && !hasFullBoundary, "Coverage boundary must include minLat, maxLat, minLng, and maxLng together.");
            AddIf(errors, $"{prefix}.boundary", hasFullBoundary && coverage.MinLat > coverage.MaxLat, "Coverage minLat must be less than or equal to maxLat.");
            AddIf(errors, $"{prefix}.boundary", hasFullBoundary && coverage.MinLng > coverage.MaxLng, "Coverage minLng must be less than or equal to maxLng.");
        }

        var duplicates = coverageAreas
            .Select((coverage, index) => new { coverage.City, coverage.Area, index })
            .Where(item => !string.IsNullOrWhiteSpace(item.City) && !string.IsNullOrWhiteSpace(item.Area))
            .GroupBy(item => $"{item.City.Trim().ToUpperInvariant()}|{item.Area.Trim().ToUpperInvariant()}")
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(item => item.index));

        foreach (var index in duplicates)
        {
            AddIf(errors, $"coverageAreas[{index}]", true, "Coverage city/area is duplicated in this request.");
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidateResolve(ResolveStoreRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        AddIf(errors, nameof(request.Lat), request.Lat is < -90 or > 90, "Latitude must be between -90 and 90.");
        AddIf(errors, nameof(request.Lng), request.Lng is < -180 or > 180, "Longitude must be between -180 and 180.");
        AddIf(errors, nameof(request.Lat), (request.Lat is null) != (request.Lng is null), "Latitude and longitude must be supplied together.");
        AddIf(errors, nameof(request.City), string.IsNullOrWhiteSpace(request.City) && request.Lat is null, "City/area or lat/lng is required.");
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
}
