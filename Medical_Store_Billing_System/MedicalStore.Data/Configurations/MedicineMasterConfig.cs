using Medical_Store_Billing_System.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class MedicineMasterConfig : IEntityTypeConfiguration<MedicineMaster>
    {
        public void Configure(EntityTypeBuilder<MedicineMaster> builder)
        {
            builder.ToTable("MedicineMaster");
            builder.HasKey(x => x.MedId);

            builder.Property(x => x.MedId).HasColumnName("MedId").UseIdentityColumn();
            builder.Property(x => x.MedName).HasColumnName("MedName").HasMaxLength(200).IsRequired();
            builder.Property(x => x.CatId).HasColumnName("CatId").IsRequired();
            builder.Property(x => x.BrandId).HasColumnName("BrandId").IsRequired();
            builder.Property(x => x.Unit).HasColumnName("Unit").HasMaxLength(50).IsRequired();
            builder.Property(x => x.PurchaseRate).HasColumnName("PurchaseRate").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.SaleRate).HasColumnName("SaleRate").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.GstPct).HasColumnName("GstPct").HasColumnType("decimal(5,2)").HasDefaultValue(0);
            builder.Property(x => x.Stock).HasColumnName("Stock").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.ExpiryDate).HasColumnName("ExpiryDate");
            builder.Property(x => x.BatchNo).HasColumnName("BatchNo").HasMaxLength(100);
            builder.Property(x => x.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
            builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate");

            // Relationships
            builder.HasOne(x => x.Category)
                   .WithMany(c => c.Medicines)
                   .HasForeignKey(x => x.CatId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Brand)
                   .WithMany(b => b.Medicines)
                   .HasForeignKey(x => x.BrandId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.CatId);
            builder.HasIndex(x => x.BrandId);
            builder.HasIndex(x => x.MedName);
        }
    }
}