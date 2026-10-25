using Microsoft.EntityFrameworkCore;
using TravelerApi.Models;

namespace TravelerApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TravelPlan> TravelPlans => Set<TravelPlan>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelPlan>(entity =>
        {
            entity.ToTable("travel_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.StartDate).HasColumnName("start_date");
            entity.Property(x => x.EndDate).HasColumnName("end_date");
            entity.Property(x => x.Budget).HasColumnName("budget").HasPrecision(10, 2);
            entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(x => x.IsPublic).HasColumnName("is_public");
            entity.Property(x => x.Version).HasColumnName("version");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TravelPlanId).HasColumnName("travel_plan_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(10, 6);
            entity.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(11, 6);
            entity.Property(x => x.VisitOrder).HasColumnName("visit_order");
            entity.Property(x => x.ArrivalDate).HasColumnName("arrival_date");
            entity.Property(x => x.DepartureDate).HasColumnName("departure_date");
            entity.Property(x => x.Budget).HasColumnName("budget").HasPrecision(10, 2);
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => new { x.TravelPlanId, x.VisitOrder }).IsUnique();
            entity.HasOne(x => x.TravelPlan).WithMany(x => x.Locations)
                .HasForeignKey(x => x.TravelPlanId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
