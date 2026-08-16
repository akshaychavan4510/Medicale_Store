using Medical_Store_Billing_System.Models;
using MedicalStore.Data.Configurations;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── Business DbSets ──────────────────────────────────────────
        public DbSet<Brand> Brands { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<MedicineCategory> MedicineCategories { get; set; }

        public DbSet<MedicineMaster> MedicineMasters { get; set; }

        public DbSet<PurchaseMaster> PurchaseMasters { get; set; }

        public DbSet<PurchaseDetails> PurchaseDetails { get; set; }

        public DbSet<SaleMaster> SaleMasters { get; set; }

        public DbSet<SaleDetails> SaleDetails { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Receipt> Receipts { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration classes
            builder.ApplyConfiguration(new MedicineCategoryConfig());
            builder.ApplyConfiguration(new BrandConfig());
            builder.ApplyConfiguration(new MedicineMasterConfig());
            builder.ApplyConfiguration(new CustomerConfig());
            builder.ApplyConfiguration(new SupplierConfig());
            builder.ApplyConfiguration(new SaleMasterConfig());
            builder.ApplyConfiguration(new SaleDetailsConfig());
            builder.ApplyConfiguration(new PurchaseMasterConfig());
            builder.ApplyConfiguration(new PurchaseDetailsConfig());
            builder.ApplyConfiguration(new ReceiptConfig());
            builder.ApplyConfiguration(new PaymentConfig());



        }

    }
}