using Identity_service.Entities;

namespace Identity_service.Features.Drivers.Applications.Submit;

public sealed record SubmitDriverApplicationResponse(Guid ApplicationId, DriverApplicationStatus Status);
