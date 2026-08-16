using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class PurchaseDetailsConfig : IEntityTypeConfiguration<PurchaseDetails>
    {
        public void Configure(EntityTypeBuilder<PurchaseDetails> builder)
        {
            builder.ToTable("PurchaseDetails");

            builder.HasKey(x => x.PurchaseDetId);

            builder.Property(x => x.PurchaseDetId)
                   .HasColumnName("PurchaseDetId")
                   .UseIdentityColumn();

            builder.Property(x => x.PurchaseId)
                   .HasColumnName("PurchaseId")
                   .IsRequired();

            builder.Property(x => x.MedId)
                   .HasColumnName("MedId")
                   .IsRequired();

            // Qty is decimal in the DB (matches SaleDetails) — must be decimal(18,2)
            builder.Property(x => x.Qty)
                   .HasColumnName("Qty")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.Rate)
                   .HasColumnName("Rate")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.Amt)
                   .HasColumnName("Amt")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.GstPct)
                   .HasColumnName("GstPct")
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0);

            builder.Property(x => x.GstAmt)
                   .HasColumnName("GstAmt")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(x => x.Total)
                   .HasColumnName("Total")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.ExpiryDate)
                   .HasColumnName("ExpiryDate");

            builder.Property(x => x.BatchNo)
                   .HasColumnName("BatchNo")
                   .HasMaxLength(100);

            // FIX: Fluent API is the ONLY FK definition — no [ForeignKey] attributes on the entity
            builder.HasOne(x => x.PurchaseMaster)
                   .WithMany(pm => pm.PurchaseDetails)
                   .HasForeignKey(x => x.PurchaseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Medicine)
                   .WithMany(m => m.PurchaseDetails)
                   .HasForeignKey(x => x.MedId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.PurchaseId);
            builder.HasIndex(x => x.MedId);
        }
    }
}