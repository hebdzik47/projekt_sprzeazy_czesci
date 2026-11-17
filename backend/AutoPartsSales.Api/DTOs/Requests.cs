using System.ComponentModel.DataAnnotations;

namespace AutoPartsSales.Api.DTOs;

public record LoginRequest([Required, StringLength(100)] string UserName, [Required] string Password);
public record RegisterRequest([Required, RegularExpression(@"^[a-zA-Z0-9._-]{3,100}$", ErrorMessage = "Login musi mieć od 3 do 100 znaków: litery, cyfry, kropkę, myślnik lub podkreślenie.")] string UserName, [Required, StringLength(100, MinimumLength = 2)] string DisplayName, [Required, MinLength(8)] string Password);
public record CreateUserRequest([Required, StringLength(100)] string UserName, [Required, StringLength(100)] string DisplayName, [Required, MinLength(8)] string Password, [Required] string Role);
public record CarRequest([Required, StringLength(100)] string Make, [Required, StringLength(100)] string Model, string? Generation, int? Year, string? EngineCode, string? Notes);
public record PartRequest([Required, StringLength(200)] string Name, string? Notes);
public record CarPartRequest([Range(1, int.MaxValue)] int PartId);
public record SaleItemRequest([Range(1, int.MaxValue)] int PartId, decimal Price, [Range(1, 10000)] int Quantity = 1);
public record BatchSaleRequest([Range(1, int.MaxValue)] int CarId, [Required, MinLength(1), MaxLength(100)] List<SaleItemRequest> Items, DateOnly SaleDate, int? SalespersonId = null, string PriceType = "net");
public record SaleRequest([Range(1, int.MaxValue)] int CarId, [Range(1, int.MaxValue)] int PartId, decimal Price, DateOnly SaleDate, int? SalespersonId = null, string PriceType = "net");
