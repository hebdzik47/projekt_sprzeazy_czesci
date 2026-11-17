using Microsoft.AspNetCore.Identity;

namespace AutoPartsSales.Api.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public int? SalespersonId { get; set; }
    public Salesperson? Salesperson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Sale> Sales { get; set; } = [];
}
