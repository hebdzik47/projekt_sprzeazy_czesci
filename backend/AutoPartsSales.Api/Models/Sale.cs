namespace AutoPartsSales.Api.Models;

public class Sale
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? OriginalGrossPrice { get; set; }
    public int? SalespersonId { get; set; }
    public Salesperson? Salesperson { get; set; }
    public DateOnly SaleDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
