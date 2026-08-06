using Identity_service.Abstractions;

namespace Identity_service.Errors;

public static class DriverApplicationErrors
{
    public static readonly Error NotFound =
        new("DriverApplication.NotFound", "Driver application was not found.", StatusCodes.Status404NotFound);

    public static readonly Error InvalidTransition =
        new("DriverApplication.InvalidTransition", "Only pending applications can be reviewed.", StatusCodes.Status409Conflict);
}
