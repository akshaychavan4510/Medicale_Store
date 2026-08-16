using Medical_Store_Billing_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalStore.Data.Configurations
{
    public class MedicineCategoryConfig : IEntityTypeConfiguration<MedicineCategory>
    {
        public void Configure(EntityTypeBuilder<MedicineCategory> builder)
        {
            // ✅ FIX: Was 'Medicine_Category', actual DB table name is 'MedicineCategory'
            builder.ToTable("MedicineCategory");
            builder.HasKey(x => x.CatId);

            builder.Property(x => x.CatId)
                   .HasColumnName("CatId")
                   .UseIdentityColumn();

            builder.Property(x => x.CatName)
                   .HasColumnName("CatName")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasColumnName("Description")
                   .HasMaxLength(250);

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