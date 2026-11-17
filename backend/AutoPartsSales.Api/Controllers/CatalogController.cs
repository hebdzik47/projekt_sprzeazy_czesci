using AutoPartsSales.Api.Data;
using AutoPartsSales.Api.DTOs;
using AutoPartsSales.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AutoPartsSales.Api.Controllers;

[ApiController, Route("api"), Authorize]
public class CatalogController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("car-makes")]
    public async Task<IActionResult> Makes()
    {
        var existing = await db.Cars.Select(x => x.Make.ToLower()).Distinct().ToListAsync();
        return Ok(Configurations.CarMakes.All.Concat(existing).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
    }

    [HttpGet("cars")]
    public async Task<IActionResult> Cars() => Ok(await db.Cars.AsNoTracking().OrderBy(x => x.Make.ToLower()).ThenBy(x => x.Model.ToLower()).ThenBy(x => x.Id).Select(x => new { x.Id, x.Make, x.Model }).ToListAsync());

    [HttpGet("salespeople"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Salespeople() => Ok(await db.Salespeople.AsNoTracking().OrderBy(x => x.Id).ToListAsync());

    [HttpGet("cars/{carId:int}/parts")]
    public async Task<IActionResult> CarParts(int carId) { if (!await db.Cars.AnyAsync(x => x.Id == carId)) return NotFound(); return await Parts(); }

    [HttpGet("parts")]
    public async Task<IActionResult> Parts() => Ok(await db.Parts.AsNoTracking().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync());

    [HttpPost("cars"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddCar(CarRequest r) { var make = r.Make.Trim(); if (make.Equals("Volkswagen", StringComparison.OrdinalIgnoreCase) || make.Equals("vw", StringComparison.OrdinalIgnoreCase)) make = "vw"; var car = new Car { Make = make, Model = r.Model.Trim() }; db.Cars.Add(car); await db.SaveChangesAsync(); return Ok(new { car.Id, car.Make, car.Model }); }

    [HttpPost("parts"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddPart(PartRequest r) { if (await db.Parts.AnyAsync(x => x.Name == r.Name.Trim())) return Conflict(new { message = "Taka część już istnieje." }); var part = new Part { Name = r.Name.Trim() }; db.Parts.Add(part); await db.SaveChangesAsync(); return Ok(new { part.Id, part.Name }); }
}
