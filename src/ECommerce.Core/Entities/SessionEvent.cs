using System;

namespace ECommerce.Core.Entities;

public class SessionEvent : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? PageVisited { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public int DurationSeconds { get; set; }
}