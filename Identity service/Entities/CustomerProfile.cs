namespace Identity_service.Entities;

/// <summary>
/// Customer-owned data used by shopping and ordering workflows.
/// </summary>
public class CustomerProfile
{
    #region Identity

    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;

    #endregion

    #region Customer details

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation properties

    public ApplicationUser? User { get; set; }

    #endregion
}
