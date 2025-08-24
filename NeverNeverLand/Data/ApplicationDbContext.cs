using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NeverNeverLand.Models;

namespace NeverNeverLand.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Models.Ticket> Ticket { get; set; }
        public DbSet<NeverNeverLand.Models.ParkPass> ParkPass { get; set; } = default!;
        public DbSet<Price> Prices => Set<Price>();
        public DbSet<PriceChangeLog> PriceChangeLogs => Set<PriceChangeLog>();
        public DbSet<Season> Seasons => Set<Season>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DateOnly <-> date (SQL) conversion
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d));

            modelBuilder.Entity<Season>(e =>
            {
                e.Property(s => s.Name).HasMaxLength(100).IsRequired();
                e.Property(s => s.StartDate).HasConversion(dateOnlyConverter).HasColumnType("date");
                e.Property(s => s.EndDate).HasConversion(dateOnlyConverter).HasColumnType("date");

                e.HasIndex(s => new { s.IsActive, s.AlwaysOn, s.StartDate, s.EndDate });
            });

            modelBuilder.Entity<Price>(e =>
            {
                e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
                e.Property(p => p.AdmissionType).HasMaxLength(50);
                e.Property(p => p.Channel).HasMaxLength(16);
                e.Property(p => p.Currency).HasMaxLength(3);

                e.HasOne(p => p.Season).WithMany().HasForeignKey(p => p.SeasonId).OnDelete(DeleteBehavior.Restrict);

                
                e.HasIndex(p => new { p.SeasonId, p.AdmissionType, p.Channel, p.IsActive });
            });

            modelBuilder.Entity<PriceChangeLog>(e =>
            {
                e.Property(x => x.OldAmount).HasColumnType("decimal(10,2)");
                e.Property(x => x.NewAmount).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Ticket>(e =>
            {
                e.Property(x => x.PricePaid).HasColumnType("decimal(10,2)");
            });
        }
    }
}
