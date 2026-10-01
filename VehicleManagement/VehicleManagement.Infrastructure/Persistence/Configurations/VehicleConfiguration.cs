using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleManagement.Domain.Entities;

namespace VehicleManagement.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.YearOfManufacture)
            .IsRequired();

        builder.Property(x => x.Weight)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ManufacturerId);

        builder.HasData(new { Id = 1, OwnerName ="Vi Vo", ManufacturerId = 1, YearOfManufacture = 2025, Weight = 1800m});
    }
}
