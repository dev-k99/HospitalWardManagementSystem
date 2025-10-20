using System;

namespace ECommerce.Core.Entities;

public class Recommendation : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public float Score { get; set; }
}