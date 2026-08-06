namespace Identity_service.Entities;

/// <summary>
/// Approved driver-owned profile data used by delivery workflows.
/// </summary>
public class DriverProfile
{
    #region Identity

    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;

    #endregion

    #region Driver details

    public VehicleType VehicleType { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation properties

    public ApplicationUser? User { get; set; }

    #endregion
}
