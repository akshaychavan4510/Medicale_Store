using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Column names use EF conventions matching the Migration
            builder.ToTable("Payment");

            builder.HasKey(x => x.PaymentId);

            builder.Property(x => x.PaymentId)
                   .UseIdentityColumn();

            builder.Property(x => x.PaymentDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.PayMode)
                   .HasMaxLength(50);      // PaymentMode in migration = PayMode here

            builder.Property(x => x.RefNo)
                   .HasMaxLength(100);

            builder.Property(x => x.Note)
                   .HasMaxLength(500);

            builder.Property(x => x.CreatedBy)
                   .HasMaxLength(450);

            builder.Property(x => x.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Supplier)
                   .WithMany(s => s.Payments)
                   .HasForeignKey(x => x.SuppId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SuppId);
        }
    }
}