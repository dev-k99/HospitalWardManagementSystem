using System;
using System.Collections.Generic;

namespace ECommerce.Core.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public decimal TotalAmount { get; set; }
    public string? StripePaymentId { get; set; }
    public string Status { get; set; } = "Pending";

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}