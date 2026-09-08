namespace Identity_service.Entities;

public class UserFCMToken: BaseEntity
{
    public string DeviceId { get; set; } 
    public string FCMToken { get; set; }
    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }
}
