namespace AutoPartsSales.Api.Models;

public class CarPart
{
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
}
