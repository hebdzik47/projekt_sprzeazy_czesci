using AutoPartsSales.Api.Data;
using AutoPartsSales.Api.DTOs;
using AutoPartsSales.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AutoPartsSales.Api.Controllers;

[ApiController, Route("api/sales"), Authorize]
public class SalesController(ApplicationDbContext db, UserManager<AppUser> users) : ControllerBase
{
    [HttpGet("sellers"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Sellers()
    {
        var sellers = await db.Salespeople.OrderBy(x => x.Id).Select(x => new { key = "seller:" + x.Id, name = x.Name }).ToListAsync();
        var legacy = await db.Sales.Where(x => x.SalespersonId == null).Select(x => new { key = "user:" + x.UserId, name = x.User.DisplayName }).Distinct().ToListAsync();
        return Ok(sellers.Concat(legacy));
    }

    [HttpGet]
    public async Task<IActionResult> Get(int page = 1, int pageSize = 20, string? search = null, int? carId = null, int? partId = null, string? userId = null, string? seller = null, DateOnly? from = null, DateOnly? to = null)
    {
        if (page < 1 || page > 1000000 || pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Nieprawidłowa strona wyników." });
        if (from > to) return BadRequest(new { message = "Data od nie może być późniejsza od daty do." });
        var q = db.Sales.AsNoTracking().AsQueryable();
        if (!User.IsInRole("Administrator"))
        {
            var current = await users.GetUserAsync(User);
            if (current == null) return Unauthorized();
            q = q.Where(x => x.UserId == current.Id || (current.SalespersonId != null && x.SalespersonId == current.SalespersonId));
        }
        else
        {
            if (userId is not null) q = q.Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(seller))
            {
                if (seller.StartsWith("seller:") && int.TryParse(seller[7..], out var id)) q = q.Where(x => x.SalespersonId == id);
                else if (seller.StartsWith("user:")) { var uid = seller[5..]; q = q.Where(x => x.SalespersonId == null && x.UserId == uid); }
                else return BadRequest(new { message = "Nieprawidłowy sprzedawca." });
            }
        }
        if (carId.HasValue) q = q.Where(x => x.CarId == carId);
        if (partId.HasValue) q = q.Where(x => x.PartId == partId);
        if (from.HasValue) q = q.Where(x => x.SaleDate >= from);
        if (to.HasValue) q = q.Where(x => x.SaleDate <= to);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.ToLower(); q = q.Where(x => x.Car.Make.ToLower().Contains(term) || x.Car.Model.ToLower().Contains(term) || x.Part.Name.ToLower().Contains(term) || (x.Salesperson != null ? x.Salesperson.Name : x.User.DisplayName).ToLower().Contains(term)); }
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.SaleDate).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.UserId, userName = x.Salesperson != null ? x.Salesperson.Name : x.User.DisplayName, x.CarId, car = x.Car.Make + " " + x.Car.Model, x.PartId, part = x.Part.Name, x.Price, x.Quantity, totalNet = x.Price * x.Quantity, x.SaleDate, x.CreatedAt }).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Add(SaleRequest r)
    {
        if (r.SalespersonId.HasValue && !User.IsInRole("Administrator")) return Forbid();
        var current = await users.GetUserAsync(User);
        if (current == null) return Unauthorized();
        var salespersonId = User.IsInRole("Administrator") ? r.SalespersonId : current.SalespersonId;
        if (User.IsInRole("Administrator") && !r.SalespersonId.HasValue) return BadRequest(new { message = "Wybierz sprzedawcę." });
        if (r.SalespersonId.HasValue && !await db.Salespeople.AnyAsync(x => x.Id == r.SalespersonId)) return BadRequest(new { message = "Nieprawidłowy sprzedawca." });
        if (r.SaleDate == default) return BadRequest(new { message = "Data sprzedaży jest wymagana." });
        if (r.PriceType is not ("net" or "gross")) return BadRequest(new { message = "Wybierz cenę netto lub brutto." });
        if (r.Price <= 0 || r.Price > 9999999999.99m || decimal.Round(r.Price, 2) != r.Price) return BadRequest(new { message = "Cena musi być dodatnia, mieć najwyżej dwa miejsca po przecinku i nie przekraczać 9 999 999 999,99 zł." });
        if (!await db.Cars.AnyAsync(x => x.Id == r.CarId) || !await db.Parts.AnyAsync(x => x.Id == r.PartId)) return BadRequest(new { message = "Wybierz istniejący samochód i część." });
        var net = r.PriceType == "gross" ? decimal.Round(r.Price / 1.23m, 2, MidpointRounding.AwayFromZero) : r.Price;
        var sale = new Sale { UserId = users.GetUserId(User)!, SalespersonId = salespersonId, CarId = r.CarId, PartId = r.PartId, Price = net, OriginalGrossPrice = r.PriceType == "gross" ? r.Price : null, SaleDate = r.SaleDate };
        db.Sales.Add(sale);
        await db.SaveChangesAsync();
        return Created($"api/sales/{sale.Id}", new { sale.Id, sale.Price });
    }

    [HttpPost("batch")]
    public async Task<IActionResult> AddBatch(BatchSaleRequest r)
    {
        if (r.SalespersonId.HasValue && !User.IsInRole("Administrator")) return Forbid();
        var current = await users.GetUserAsync(User);
        if (current == null) return Unauthorized();
        var salespersonId = User.IsInRole("Administrator") ? r.SalespersonId : current.SalespersonId;
        if (User.IsInRole("Administrator") && !r.SalespersonId.HasValue) return BadRequest(new { message = "Wybierz sprzedawcę." });
        if (r.SalespersonId.HasValue && !await db.Salespeople.AnyAsync(x => x.Id == r.SalespersonId)) return BadRequest(new { message = "Nieprawidłowy sprzedawca." });
        if (r.SaleDate == default) return BadRequest(new { message = "Data sprzedaży jest wymagana." });
        if (r.PriceType is not ("net" or "gross")) return BadRequest(new { message = "Wybierz cenę netto lub brutto." });
        if (r.Items is null || r.Items.Count is < 1 or > 100 || r.Items.Any(x => x is null)) return BadRequest(new { message = "Wybierz od 1 do 100 części." });
        if (r.Items.Any(x => x.Quantity < 1 || x.Quantity > 10000)) return BadRequest(new { message = "Ilość musi być całkowitą liczbą od 1 do 10 000." });
        if (r.Items.Any(x => x.Price <= 0 || x.Price > 9999999999.99m || decimal.Round(x.Price, 2) != x.Price)) return BadRequest(new { message = "Każda część musi mieć dodatnią cenę z maksymalnie dwoma miejscami po przecinku, do 9 999 999 999,99 zł." });
        var ids = r.Items.Select(x => x.PartId).Distinct().ToArray();
        if (ids.Length != r.Items.Count) return BadRequest(new { message = "Ta sama część została wybrana więcej niż raz." });
        if (!await db.Cars.AnyAsync(x => x.Id == r.CarId) || await db.Parts.CountAsync(x => ids.Contains(x.Id)) != ids.Length) return BadRequest(new { message = "Wybierz istniejący samochód i części." });
        var createdAt = DateTime.UtcNow;
        var sales = r.Items.Select(x => new Sale {
            UserId = users.GetUserId(User)!, SalespersonId = salespersonId, CarId = r.CarId,
            PartId = x.PartId, Quantity = x.Quantity, SaleDate = r.SaleDate, CreatedAt = createdAt,
            Price = r.PriceType == "gross" ? decimal.Round(x.Price / 1.23m, 2, MidpointRounding.AwayFromZero) : x.Price,
            OriginalGrossPrice = r.PriceType == "gross" ? x.Price : null
        }).ToList();
        db.Sales.AddRange(sales);
        // One SaveChanges call is transactional: either every item is saved, or none.
        await db.SaveChangesAsync();
        return StatusCode(201, new { ids = sales.Select(x => x.Id), count = sales.Count, quantity = sales.Sum(x => x.Quantity), totalNet = sales.Sum(x => x.Price * x.Quantity) });
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id) { var sale = await db.Sales.FindAsync(id); if (sale is null) return NotFound(); db.Sales.Remove(sale); await db.SaveChangesAsync(); return NoContent(); }
}
