using AutoPartsSales.Api.DTOs;
using AutoPartsSales.Api.Models;
using AutoPartsSales.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsSales.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(UserManager<AppUser> users, RoleManager<IdentityRole> roles, TokenService tokens) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var userName = request.UserName.Trim();
        if (await users.FindByNameAsync(userName) is not null)
            return Conflict(new { message = "Ten login jest już zajęty." });

        var user = new AppUser { UserName = userName, DisplayName = request.DisplayName.Trim() };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(error => error.Description)) });

        const string employeeRole = "Employee";
        if (!await roles.RoleExistsAsync(employeeRole))
            await roles.CreateAsync(new IdentityRole(employeeRole));
        var roleResult = await users.AddToRoleAsync(user, employeeRole);
        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);
            return Problem("Nie udało się utworzyć konta. Spróbuj ponownie.");
        }

        return Ok(await Profile(user));
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await users.FindByNameAsync(request.UserName.Trim());
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Nieprawidłowy login lub hasło." });
        return Ok(await Profile(user));
    }

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(await Profile(user));
    }

    private async Task<object> Profile(AppUser user) => new
    {
        token = await tokens.CreateAsync(user),
        user = new { id = user.Id, userName = user.UserName, displayName = user.DisplayName, roles = await users.GetRolesAsync(user) }
    };
}