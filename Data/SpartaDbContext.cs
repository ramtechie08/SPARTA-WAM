using Microsoft.EntityFrameworkCore;
using SPARTA_WAM.Data.Models;

namespace SPARTA_WAM.Data;

public class SpartaDbContext : DbContext
{
    public SpartaDbContext(DbContextOptions<SpartaDbContext> options) : base(options)
    {
    }

    public DbSet<SpartaWalkAwayMargin> WalkAwayMargins { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SpartaWalkAwayMargin>(entity =>
        {
            entity.ToTable("tb_SPARTA_WalkAwayfloorMargin_Mst");

            entity.HasKey(e => new { e.ShortCode, e.SalesOrg, e.DirectPricing })
                .HasName("PK_WalkAwayMargin");

            entity.Property(e => e.ShortCode)
                .HasColumnName("Short_Code")
                .HasColumnType("varchar(20)")
                .IsRequired();

            entity.Property(e => e.WalkAwayMargin)
                .HasColumnType("decimal(5, 2)")
                .IsRequired();

            entity.Property(e => e.CreatedDateTime)
                .HasColumnName("Created_Datetime")
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(e => e.UpdatedDateTime)
                .HasColumnName("Updated_Datetime")
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(e => e.SalesOrg)
                .HasColumnName("Sales_Org")
                .HasColumnType("varchar(50)")
                .IsRequired();

            entity.Property(e => e.DirectPricing)
                .HasColumnName("Direct_Pricing")
                .HasColumnType("char(1)")
                .IsRequired();
        });
    }
}