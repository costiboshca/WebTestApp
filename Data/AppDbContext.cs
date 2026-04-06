using Microsoft.EntityFrameworkCore;
using WebTestApp.Models;

namespace WebTestApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Article> Articles  => Set<Article>();
    public DbSet<User>    Users     => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.Address).HasMaxLength(500);

            // Implicit many-to-many with Article via join table "CompanyArticle"
            e.HasMany(c => c.Articles)
             .WithMany(a => a.Companies)
             .UsingEntity(j => j.ToTable("CompanyArticle"));
        });

        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Code).IsRequired().HasMaxLength(100);
            e.HasIndex(a => a.Code).IsUnique();
            e.Property(a => a.Description).HasMaxLength(1000);
            e.Property(a => a.ProductCode).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });
    }
}
