using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Postal.Api.Domain;

namespace Postal.Api.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.Property(s => s.ShipmentNumber).HasMaxLength(12).IsRequired();
        builder.HasIndex(s => s.ShipmentNumber).IsUnique();

        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.WeightKg).HasPrecision(8, 3);

        // Derived values are computed in C#, not stored.
        builder.Ignore(s => s.WeightCategory);
        builder.Ignore(s => s.CurrentPostOfficeId);

        // A post office can't be deleted while shipments reference it.
        builder.HasOne(s => s.Origin).WithMany()
            .HasForeignKey(s => s.OriginPostOfficeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Destination).WithMany()
            .HasForeignKey(s => s.DestinationPostOfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("CK_Shipments_WeightKg_Positive", "\"WeightKg\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Shipments_Origin_Not_Destination",
            "\"OriginPostOfficeId\" <> \"DestinationPostOfficeId\""));

        builder.HasIndex(s => s.CreatedAt);
    }
}
