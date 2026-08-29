namespace Cart_Service.Entities;

public sealed class Cart
{
    public Guid Id { get; set; }

    /// <summary>Identity user id. One cart per user, enforced by a unique index.</summary>
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<CartItem> Items { get; set; } = [];
}
