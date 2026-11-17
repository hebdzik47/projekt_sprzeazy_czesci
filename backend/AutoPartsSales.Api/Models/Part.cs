namespace AutoPartsSales.Api.Models;

public class Part
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<CarPart> CarParts { get; set; } = [];
}
