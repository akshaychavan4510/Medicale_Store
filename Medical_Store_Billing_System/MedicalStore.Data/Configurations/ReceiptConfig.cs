using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class ReceiptConfig : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder.ToTable("Receipt");

            builder.HasKey(x => x.ReceiptId);

            builder.Property(x => x.ReceiptId)
                   .UseIdentityColumn();

            builder.Property(x => x.ReceiptDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(x => x.PayMode)
                   .HasMaxLength(50);

            builder.Property(x => x.RefNo)
                   .HasMaxLength(100);

            builder.Property(x => x.Note)
                   .HasMaxLength(500);

            builder.Property(x => x.CreatedBy)
                   .HasMaxLength(450);

            builder.Property(x => x.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Customer)
                   .WithMany(c => c.Receipts)
                   .HasForeignKey(x => x.CustId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CustId);
        }
    }
}