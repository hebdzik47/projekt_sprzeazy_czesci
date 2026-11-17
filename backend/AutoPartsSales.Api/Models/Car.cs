namespace AutoPartsSales.Api.Models;

public class Car
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Generation { get; set; }
    public int? Year { get; set; }
    public string? EngineCode { get; set; }
    public string? Notes { get; set; }
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<CarPart> CarParts { get; set; } = [];
}
