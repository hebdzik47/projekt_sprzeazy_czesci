using AutoPartsSales.Api.Data;
using AutoPartsSales.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsSales.Api.Services;

// Explicit one-time CLI operation. Never resets passwords during ordinary startup.
public static class TeamProvisioner
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        using var transaction = await db.Database.BeginTransactionAsync();
        var originalLength = users.Options.Password.RequiredLength;
        try
        {
            // User-requested temporary passwords only; normal policy stays at 8.
            users.Options.Password.RequiredLength = 5;
            foreach (var (login, name, sellerId) in new[] { ("Krystian", "Krystian", 1), ("Adam", "Adam", 2), ("Damian", "Damian", 3), ("Pawel", "Paweł", 4) })
            {
                var existing = await users.FindByNameAsync(login);
                if (existing != null)
                {
                    if (existing.SalespersonId != sellerId || !await users.IsInRoleAsync(existing, "Employee") || await users.IsInRoleAsync(existing, "Administrator"))
                        throw new InvalidOperationException($"Konto {login} już istnieje z inną konfiguracją. Nie zmieniono go.");
                    continue;
                }
                if (!await db.Salespeople.AnyAsync(x => x.Id == sellerId && x.Name == name)) throw new InvalidOperationException("Nieprawidłowy słownik sprzedawców.");
                var password = configuration[$"TeamPasswords:{login}"] ?? throw new InvalidOperationException($"Brak hasła dla {login}.");
                var user = new AppUser { UserName = login, DisplayName = name, SalespersonId = sellerId };
                var created = await users.CreateAsync(user, password);
                if (!created.Succeeded) throw new InvalidOperationException($"Nie udało się utworzyć {login}: {string.Join(", ", created.Errors.Select(x => x.Code))}");
                var role = await users.AddToRoleAsync(user, "Employee");
                if (!role.Succeeded) throw new InvalidOperationException($"Nie udało się nadać roli {login}.");
            }
            await transaction.CommitAsync();
        }
        finally { users.Options.Password.RequiredLength = originalLength; }
    }
}
