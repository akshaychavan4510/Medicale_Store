using Medical_Store_Billing_System.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");
            builder.HasKey(x => x.CustId);
            builder.Property(x => x.CustBal).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");
        }
    }
}