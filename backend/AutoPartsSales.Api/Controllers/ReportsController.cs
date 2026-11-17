using AutoPartsSales.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AutoPartsSales.Api.Controllers;

[ApiController, Route("api/reports"), Authorize(Roles = "Administrator")]
public class ReportsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(DateOnly? from, DateOnly? to)
    {
        if (from > to) return BadRequest(new { message = "Data od nie może być późniejsza od daty do." });
        var q = db.Sales.AsNoTracking();
        if (from.HasValue) q = q.Where(x => x.SaleDate >= from);
        if (to.HasValue) q = q.Where(x => x.SaleDate <= to);
        return Ok(new {
            totalValue = await q.SumAsync(x => (decimal?)x.Price * x.Quantity) ?? 0,
            salesCount = await q.CountAsync(),
            partsCount = await q.SumAsync(x => (int?)x.Quantity) ?? 0,
            byUser = await q.GroupBy(x => new { x.SalespersonId, UserId = x.SalespersonId == null ? x.UserId : null, Name = x.Salesperson != null ? x.Salesperson.Name : x.User.DisplayName })
                .Select(g => new { name = g.Key.Name, value = g.Sum(x => x.Price * x.Quantity), count = g.Sum(x => x.Quantity) }).OrderByDescending(x => x.value).ToListAsync(),
            byCar = await q.GroupBy(x => new { x.CarId, x.Car.Make, x.Car.Model })
                .Select(g => new { name = g.Key.Make + " " + g.Key.Model, value = g.Sum(x => x.Price * x.Quantity), count = g.Sum(x => x.Quantity) }).OrderByDescending(x => x.value).ToListAsync()
        });
    }
}
