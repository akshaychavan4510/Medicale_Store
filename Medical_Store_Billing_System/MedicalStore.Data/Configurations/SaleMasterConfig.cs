using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class SaleMasterConfig : IEntityTypeConfiguration<SaleMaster>
    {
        public void Configure(EntityTypeBuilder<SaleMaster> builder)
        {
            // Table name matches the Migration: "SaleMaster" (NOT "Sale_Master")
            builder.ToTable("SaleMaster");

            builder.HasKey(x => x.SaleId);

            builder.Property(x => x.SaleId)
                   .UseIdentityColumn();

            builder.Property(x => x.SaleDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.CustId)
                   .IsRequired();

            builder.Property(x => x.GrandTotal)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(x => x.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Customer)
                   .WithMany(c => c.Sales)
                   .HasForeignKey(x => x.CustId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CustId);
            builder.HasIndex(x => x.SaleDate);
        }
    }
}