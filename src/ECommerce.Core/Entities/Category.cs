using System.Collections.Generic;

namespace ECommerce.Core.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}