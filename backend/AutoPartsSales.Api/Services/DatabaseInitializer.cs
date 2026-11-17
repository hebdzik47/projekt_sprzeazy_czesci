using AutoPartsSales.Api.Data;
using AutoPartsSales.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsSales.Api.Services;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync("Administrator"))
            await roles.CreateAsync(new IdentityRole("Administrator"));
        if (!await roles.RoleExistsAsync("Employee"))
            await roles.CreateAsync(new IdentityRole("Employee"));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var login = configuration["InitialAdmin:UserName"];
        var password = configuration["InitialAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password) && await userManager.FindByNameAsync(login) is null)
        {
            var admin = new AppUser { UserName = login, DisplayName = "Administrator" };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded) throw new InvalidOperationException("Nie udało się utworzyć konta początkowego administratora.");
            await userManager.AddToRoleAsync(admin, "Administrator");
        }

        await SeedEmployeeAsync(userManager, configuration);
        await SeedCatalogAsync(db);
    }

    private static async Task SeedEmployeeAsync(UserManager<AppUser> users, IConfiguration config)
    {
        var login = config["InitialEmployee:UserName"];
        var password = config["InitialEmployee:Password"];
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || await users.FindByNameAsync(login) is not null) return;
        var employee = new AppUser { UserName = login, DisplayName = config["InitialEmployee:DisplayName"] ?? login };
        if ((await users.CreateAsync(employee, password)).Succeeded) await users.AddToRoleAsync(employee, "Employee");
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext db)
    {
        if (!await db.Cars.AnyAsync()) db.Cars.AddRange(new Car { Make = "Toyota", Model = "Yaris" }, new Car { Make = "vw", Model = "Golf" }, new Car { Make = "BMW", Model = "E90" }, new Car { Make = "Audi", Model = "A4" });
        if (!await db.Parts.AnyAsync()) db.Parts.AddRange(new Part { Name = "Lewa lampa przednia" }, new Part { Name = "Prawa lampa przednia" }, new Part { Name = "Zderzak przedni" }, new Part { Name = "Maska" }, new Part { Name = "Drzwi lewe" }, new Part { Name = "Drzwi prawe" }, new Part { Name = "Błotnik przedni" });
        await db.SaveChangesAsync();

    }
}
