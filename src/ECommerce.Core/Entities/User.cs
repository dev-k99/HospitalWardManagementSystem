using System;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}