using Identity_service.Domain.Common;
using Identity_service.Domain.Enums;

namespace Identity_service.Entities;

public class VerificationCode : BaseEntity
{
    public Guid UserId { get; set; }

    public string CodeHash { get; set; } = null!;

    public VerificationPurpose Purpose { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? ConsumedAt { get; set; }

    public int AttemptCount { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public bool IsActive => ConsumedAt is null && !IsExpired;
}
