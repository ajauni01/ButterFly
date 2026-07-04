namespace Butterfly.Data.Entities;

/// <summary>
/// Base for all entities: GUID PK plus created/updated audit timestamps.
/// <see cref="CreatedAt"/> and <see cref="UpdatedAt"/> are stamped centrally in
/// <c>ButterflyDbContext.SaveChanges</c>.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
