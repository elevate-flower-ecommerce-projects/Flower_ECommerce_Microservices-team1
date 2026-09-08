namespace Identity_service.Entities;

public class UserFCMToken: BaseEntity
{
    public string DeviceId { get; set; } 
    public string FCMToken { get; set; }
    public string UserId { get; set; }

    public ApplicationUser? User { get; set; }
}
