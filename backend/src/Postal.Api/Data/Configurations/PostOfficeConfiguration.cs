using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Postal.Api.Domain;

namespace Postal.Api.Data.Configurations;

public class PostOfficeConfiguration : IEntityTypeConfiguration<PostOffice>
{
    public void Configure(EntityTypeBuilder<PostOffice> builder)
    {
        builder.Property(p => p.ZipCode).HasMaxLength(5).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.ZipCode).IsUnique();
    }
}
