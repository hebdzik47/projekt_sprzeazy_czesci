using AutoPartsSales.Api.Data;
using AutoPartsSales.Api.Models;
using AutoPartsSales.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddIdentityCore<AppUser>(o => { o.Password.RequiredLength = 8; o.Password.RequireDigit = true; o.Password.RequireUppercase = false; o.Password.RequireLowercase = false; o.Password.RequireNonAlphanumeric = false; }).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.FromMinutes(1) });
builder.Services.AddAuthorization(); builder.Services.AddScoped<TokenService>(); builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddExceptionHandler(o => o.ExceptionHandlingPath = "/error"); builder.Services.AddProblemDetails();
var app = builder.Build();
await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);
if (args.Contains("--provision-team"))
{
    await TeamProvisioner.RunAsync(app.Services, app.Configuration);
    Console.WriteLine("Konta zespołu zostały sprawdzone i utworzone. Istniejące hasła pozostały bez zmian.");
    return;
}
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors("frontend"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.Map("/error", () => Results.Problem("Wystąpił nieoczekiwany błąd serwera.")).ExcludeFromDescription();
app.MapFallbackToFile("index.html");
app.Run();
