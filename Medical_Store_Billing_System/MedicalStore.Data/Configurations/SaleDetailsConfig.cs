using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class SaleDetailsConfig : IEntityTypeConfiguration<SaleDetails>
    {
        public void Configure(EntityTypeBuilder<SaleDetails> builder)
        {
            // ── Table ─────────────────────────────────────────────────────────
            // DB screenshot shows table is "SaleDetails" (check your actual DB name)
            // Change "SaleDetails" below to match yours exactly.
            builder.ToTable("SaleDetails");

            // ── PK ────────────────────────────────────────────────────────────
            builder.HasKey(x => x.SaleDetId);

            builder.Property(x => x.SaleDetId)
                   .HasColumnName("SaleDetId")
                   .UseIdentityColumn();

            // ── Columns ───────────────────────────────────────────────────────
            builder.Property(x => x.SaleId)
                   .HasColumnName("SaleId")
                   .IsRequired();

            builder.Property(x => x.MedId)
                   .HasColumnName("MedId")
                   .IsRequired();

            builder.Property(x => x.Rate)
                   .HasColumnName("Rate")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.Qty)
                   .HasColumnName("Qty")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.Amt)
                   .HasColumnName("Amt")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.GstPct)
                   .HasColumnName("GstPct")
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0m);

            // ✅ FIX: was x.Gst — renamed to x.GstAmt to match the entity property
            //         and the actual DB column name "GstAmt" (visible in your screenshot)
            builder.Property(x => x.GstAmt)
                   .HasColumnName("GstAmt")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.Total)
                   .HasColumnName("Total")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            // ── Relationships ─────────────────────────────────────────────────
            builder.HasOne(x => x.SaleMaster)
                   .WithMany(s => s.SaleDetails)
                   .HasForeignKey(x => x.SaleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Medicine)
                   .WithMany(m => m.SaleDetails)
                   .HasForeignKey(x => x.MedId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(x => x.SaleId);
            builder.HasIndex(x => x.MedId);
        }
    }
}