using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleManagement.Domain.Entities;

namespace VehicleManagement.Infrastructure.Persistence.Configurations;

public sealed class VehicleCategoryConfiguration : IEntityTypeConfiguration<VehicleCategory>
{
    public void Configure(EntityTypeBuilder<VehicleCategory> builder)
    {
        builder.ToTable("VehicleCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.MinWeight)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.MaxWeight)
            .HasPrecision(10, 2);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(100);

        // Default configuration: ranges from 0 kg with an open-ended top category.
        // MaxWeight is inclusive and weights have 2 decimals, so neighbours end
        // 0.01 below the next minimum (no shared weight, no gap).
        builder.HasData(
            new { Id = 1, Name = "Light", MinWeight = 0m, MaxWeight = (decimal?)499.99m, Icon = "light_icon" },
            new { Id = 2, Name = "Medium", MinWeight = 500m, MaxWeight = (decimal?)2499.99m, Icon = "medium_icon" },
            new { Id = 3, Name = "Heavy", MinWeight = 2500m, MaxWeight = (decimal?)null, Icon = "heavy_icon" });
    }
}
