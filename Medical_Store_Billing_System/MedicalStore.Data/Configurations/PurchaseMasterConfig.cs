using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class PurchaseMasterConfig : IEntityTypeConfiguration<PurchaseMaster>
    {
        public void Configure(EntityTypeBuilder<PurchaseMaster> builder)
        {
            builder.ToTable("PurchaseMaster");
            builder.HasKey(x => x.PurchaseId);

            builder.Property(x => x.PurchaseId).HasColumnName("PurchaseId").UseIdentityColumn();
            builder.Property(x => x.PurchaseDate).HasColumnName("PurchaseDate").IsRequired().HasDefaultValueSql("GETDATE()");
            builder.Property(x => x.SuppId).HasColumnName("SuppId").IsRequired();
            builder.Property(x => x.InvoiceNo).HasColumnName("InvoiceNo").HasMaxLength(100);
            builder.Property(x => x.GrandTotal).HasColumnName("GrandTotal").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.Discount).HasColumnName("Discount").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.NetTotal).HasColumnName("NetTotal").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.Note).HasColumnName("Note").HasMaxLength(500);
            builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(450);
            builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate");

            // Relationships
            builder.HasOne(x => x.Supplier)
                   .WithMany(s => s.PurchaseMasters)
                   .HasForeignKey(x => x.SuppId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.SuppId);
            builder.HasIndex(x => x.PurchaseDate);
            builder.HasIndex(x => x.InvoiceNo);
        }
    }
}