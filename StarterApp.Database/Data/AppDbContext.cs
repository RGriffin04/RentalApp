using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data;

public class AppDbContext : DbContext
{

    public AppDbContext()
    { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("RentalApp.Database.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            connectionString = config.GetConnectionString("DevelopmentConnection");
        }

        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.UseNetTopologySuite() // Enable PostGIS support
        )
        .ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
        );
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemImage> ItemImages { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserRole entity
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Item entity
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasIndex(e => e.CreatedDate);

            // Spatial index on location for fast geographic queries using PostGIS
            entity.HasIndex(e => e.Location)
                  .HasMethod("gist"); // GiST index for geography

            entity.Property(e => e.DailyPrice)
                  .HasPrecision(10, 2);

            entity.HasOne(i => i.Owner)
                  .WithMany()
                  .HasForeignKey(i => i.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Category)
                  .WithMany(c => c.Items)
                  .HasForeignKey(i => i.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ItemImage entity
        modelBuilder.Entity<ItemImage>(entity =>
        {
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => new { e.ItemId, e.IsPrimary })
                  .HasFilter("\"IsPrimary\" = true");

            entity.HasOne(ii => ii.Item)
                  .WithMany(i => i.ItemImages)
                  .HasForeignKey(ii => ii.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Rental entity
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.RenterId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);

            entity.Property(e => e.TotalPrice)
                  .HasPrecision(10, 2);

            entity.HasOne(r => r.Item)
                  .WithMany(i => i.Rentals)
                  .HasForeignKey(r => r.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Renter)
                  .WithMany()
                  .HasForeignKey(r => r.RenterId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Rating entity
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasIndex(e => e.RentalId).IsUnique();
            entity.HasIndex(e => e.RatedUserId);
            entity.HasIndex(e => e.RaterId);

            entity.HasOne(r => r.Rental)
                  .WithMany(ren => ren.Ratings)
                  .HasForeignKey(r => r.RentalId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Rater)
                  .WithMany()
                  .HasForeignKey(r => r.RaterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.RatedUser)
                  .WithMany()
                  .HasForeignKey(r => r.RatedUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
            new Category { Id = 2, Name = "Tools", Description = "Power tools, hand tools, and equipment" },
            new Category { Id = 3, Name = "Vehicles", Description = "Cars, bikes, and transportation" },
            new Category { Id = 4, Name = "Sports", Description = "Sports equipment and gear" },
            new Category { Id = 5, Name = "Home & Garden", Description = "Home improvement and gardening equipment" },
            new Category { Id = 6, Name = "Photography", Description = "Cameras, lenses, and photography equipment" },
            new Category { Id = 7, Name = "Music", Description = "Musical instruments and audio equipment" },
            new Category { Id = 8, Name = "Party & Events", Description = "Party supplies and event equipment" },
            new Category { Id = 9, Name = "Camping & Outdoor", Description = "Camping gear and outdoor equipment" },
            new Category { Id = 10, Name = "Other", Description = "Miscellaneous items" }
        );
    }
}