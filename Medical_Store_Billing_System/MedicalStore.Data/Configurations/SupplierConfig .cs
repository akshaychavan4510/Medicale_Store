using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class SupplierConfig : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Supplier");
            builder.HasKey(x => x.SuppId);

            // ✅ FIX: Use exact column names matching your DB (PascalCase, not snake_case)
            builder.Property(x => x.SuppId)
                   .HasColumnName("SuppId")
                   .UseIdentityColumn();

            builder.Property(x => x.SuppName)
                   .HasColumnName("SuppName")
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.SuppPhone)
                   .HasColumnName("SuppPhone")
                   .HasMaxLength(15);

            builder.Property(x => x.SuppEmail)
                   .HasColumnName("SuppEmail")
                   .HasMaxLength(100);

            builder.Property(x => x.SuppAddress)
                   .HasColumnName("SuppAddress")
                   .HasMaxLength(250);

            builder.Property(x => x.GstNo)
                   .HasColumnName("GstNo")
                   .HasMaxLength(50);

            builder.Property(x => x.SuppBal)
                   .HasColumnName("SuppBal")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(x => x.IsActive)
                   .HasColumnName("IsActive")
                   .HasDefaultValue(true);

            builder.Property(x => x.CreatedDate)
                   .HasColumnName("CreatedDate")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.ModifiedDate)
                   .HasColumnName("ModifiedDate");
        }
    }
}