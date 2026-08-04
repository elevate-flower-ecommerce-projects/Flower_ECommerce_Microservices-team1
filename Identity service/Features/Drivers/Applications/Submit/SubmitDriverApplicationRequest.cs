using Identity_service.Entities;

namespace Identity_service.Features.Drivers.Applications.Submit;

public sealed class SubmitDriverApplicationRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string VehiclePlateNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public List<IFormFile>? Documents { get; set; }
}
