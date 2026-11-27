using Catalog3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog3D.Infrastructure.Persistence.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Key)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(a => a.Value)
            .IsRequired()
            .HasMaxLength(500);
    }
}