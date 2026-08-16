using Medical_Store_Billing_System.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{

    public class BrandConfig : IEntityTypeConfiguration<Brand>  // ← class was missing entirely
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brand");
            builder.HasKey(x => x.BrandId);
            builder.Property(x => x.BrandId).HasColumnName("BrandId").UseIdentityColumn();
            builder.Property(x => x.BrandName).HasColumnName("BrandName").HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.BrandName).IsUnique();
        }
    }
}