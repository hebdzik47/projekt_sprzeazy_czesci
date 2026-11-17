using AutoPartsSales.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsSales.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Salesperson> Salespeople => Set<Salesperson>();
    public DbSet<CarPart> CarParts => Set<CarPart>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().HasOne(x => x.Salesperson).WithMany().HasForeignKey(x => x.SalespersonId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<AppUser>().HasIndex(x => x.SalespersonId).IsUnique();
        builder.Entity<Salesperson>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasData(new Salesperson { Id = 1, Name = "Krystian" }, new Salesperson { Id = 2, Name = "Adam" }, new Salesperson { Id = 3, Name = "Damian" }, new Salesperson { Id = 4, Name = "Paweł" }, new Salesperson { Id = 5, Name = "sprzedaż na miejscu" });
        });
        builder.Entity<Car>(entity => { entity.Property(x => x.Make).HasMaxLength(100).IsRequired(); entity.Property(x => x.Model).HasMaxLength(100).IsRequired(); entity.HasIndex(x => new { x.Make, x.Model }); });
        builder.Entity<Part>(entity => { entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.HasIndex(x => x.Name).IsUnique(); });
        builder.Entity<Sale>(entity =>
        {
            entity.Property(x => x.Price).HasPrecision(12, 2);
            entity.Property(x => x.Quantity).HasDefaultValue(1);
            entity.ToTable(t => t.HasCheckConstraint("CK_Sales_Quantity", "\"Quantity\" BETWEEN 1 AND 10000"));
            entity.Property(x => x.OriginalGrossPrice).HasPrecision(12, 2);
            entity.HasOne(x => x.Salesperson).WithMany().HasForeignKey(x => x.SalespersonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.SaleDate); entity.HasIndex(x => x.UserId); entity.HasIndex(x => x.CarId); entity.HasIndex(x => x.PartId);
            entity.HasOne(x => x.Car).WithMany(x => x.Sales).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Part).WithMany(x => x.Sales).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.User).WithMany(x => x.Sales).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<CarPart>(entity =>
        {
            entity.HasKey(x => new { x.CarId, x.PartId });
            entity.HasOne(x => x.Car).WithMany(x => x.CarParts).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part).WithMany(x => x.CarParts).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
