using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicalStore.MedicalStore.Data.Speed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // ── Ensure database exists and all migrations are applied ──────
                // If migration already ran (even partially), this is safe to call again.
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Migration may fail if schema was created manually via SQL scripts.
                // Log the warning but continue — the tables already exist.
                logger.LogWarning(ex,
                    "MigrateAsync encountered an error (schema may already exist). Continuing with seeding.");
            }

            try
            {
                // ── Roles ─────────────────────────────────────────────────────
                string[] roles = { "Admin", "Pharmacist", "Accountant" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (result.Succeeded)
                            logger.LogInformation("Role '{Role}' created.", role);
                        else
                            logger.LogError("Failed to create role '{Role}': {Errors}",
                                role, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // ── Admin User ────────────────────────────────────────────────
                const string adminEmail = "admin@medstore.com";
                const string adminPassword = "Admin@123";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    // Fresh install — create the admin user
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Admin",
                        EmailConfirmed = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation(
                            "Admin user '{Email}' created and assigned Admin role.", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // User already exists — force-reset the password so any
                    // manually inserted SQL hash is replaced with a valid one.
                    logger.LogInformation(
                        "Admin user '{Email}' already exists — resetting password to ensure correct hash.",
                        adminEmail);

                    // Ensure EmailConfirmed and FullName are set correctly
                    adminUser.EmailConfirmed = true;
                    adminUser.FullName = "System Admin";
                    await userManager.UpdateAsync(adminUser);

                    // Remove old password and set a fresh one via Identity
                    var removeResult = await userManager.RemovePasswordAsync(adminUser);
                    if (removeResult.Succeeded)
                    {
                        var addResult = await userManager.AddPasswordAsync(adminUser, adminPassword);
                        if (addResult.Succeeded)
                            logger.LogInformation("Admin password reset successfully.");
                        else
                            logger.LogError("Failed to set admin password: {Errors}",
                                string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogError("Failed to remove old admin password: {Errors}",
                            string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }

                    // Ensure Admin role is assigned
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin role assigned to existing user.");
                    }
                }

                // ── Medicine Categories ───────────────────────────────────────
                if (!await context.MedicineCategories.AnyAsync())
                {
                    await context.MedicineCategories.AddRangeAsync(
                        new MedicineCategory { CatName = "Tablet" },
                        new MedicineCategory { CatName = "Capsule" },
                        new MedicineCategory { CatName = "Syrup" },
                        new MedicineCategory { CatName = "Injection" },
                        new MedicineCategory { CatName = "Ointment" }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Medicine categories seeded.");
                }

                // ── Brands ────────────────────────────────────────────────────
                if (!await context.Brands.AnyAsync())
                {
                    await context.Brands.AddRangeAsync(
                        new Brand { BrandName = "Cipla" },
                        new Brand { BrandName = "Sun Pharma" },
                        new Brand { BrandName = "Dr. Reddy's" },
                        new Brand { BrandName = "Mankind" },
                        new Brand { BrandName = "GSK" }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Brands seeded.");
                }

                // ── Medicines ─────────────────────────────────────────────────
                if (!await context.MedicineMasters.AnyAsync())
                {
                    var tablet = await context.MedicineCategories.FirstAsync(x => x.CatName == "Tablet");
                    var capsule = await context.MedicineCategories.FirstAsync(x => x.CatName == "Capsule");
                    var syrup = await context.MedicineCategories.FirstAsync(x => x.CatName == "Syrup");
                    var ointment = await context.MedicineCategories.FirstAsync(x => x.CatName == "Ointment");

                    var cipla = await context.Brands.FirstAsync(x => x.BrandName == "Cipla");
                    var sun = await context.Brands.FirstAsync(x => x.BrandName == "Sun Pharma");
                    var reddy = await context.Brands.FirstAsync(x => x.BrandName == "Dr. Reddy's");
                    var mankind = await context.Brands.FirstAsync(x => x.BrandName == "Mankind");
                    var gsk = await context.Brands.FirstAsync(x => x.BrandName == "GSK");

                    await context.MedicineMasters.AddRangeAsync(
                        new MedicineMaster
                        {
                            MedName = "Paracetamol 500mg",
                            CatId = tablet.CatId,
                            BrandId = cipla.BrandId,
                            Unit = "Strip",
                            PurchaseRate = 2.00m,
                            SaleRate = 2.50m,
                            GstPct = 5m,
                            Stock = 500
                        },
                        new MedicineMaster
                        {
                            MedName = "Amoxicillin 250mg",
                            CatId = capsule.CatId,
                            BrandId = sun.BrandId,
                            Unit = "Strip",
                            PurchaseRate = 6.50m,
                            SaleRate = 8.00m,
                            GstPct = 12m,
                            Stock = 300
                        },
                        new MedicineMaster
                        {
                            MedName = "Cough Syrup 100ml",
                            CatId = syrup.CatId,
                            BrandId = reddy.BrandId,
                            Unit = "Bottle",
                            PurchaseRate = 38.00m,
                            SaleRate = 45.00m,
                            GstPct = 12m,
                            Stock = 120
                        },
                        new MedicineMaster
                        {
                            MedName = "Vitamin C Tablet",
                            CatId = tablet.CatId,
                            BrandId = mankind.BrandId,
                            Unit = "Strip",
                            PurchaseRate = 2.40m,
                            SaleRate = 3.00m,
                            GstPct = 5m,
                            Stock = 400
                        },
                        new MedicineMaster
                        {
                            MedName = "Pain Relief Gel",
                            CatId = ointment.CatId,
                            BrandId = gsk.BrandId,
                            Unit = "Tube",
                            PurchaseRate = 48.00m,
                            SaleRate = 60.00m,
                            GstPct = 12m,
                            Stock = 80
                        }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Medicines seeded.");
                }

                // ── Customers ─────────────────────────────────────────────────
                if (!await context.Customers.AnyAsync())
                {
                    await context.Customers.AddRangeAsync(
                        new Customer { CustName = "Rahul Sharma", CustPhone = "9876543210" },
                        new Customer { CustName = "Priya Patel", CustPhone = "9876543211" }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Customers seeded.");
                }

                // ── Suppliers ─────────────────────────────────────────────────
                if (!await context.Suppliers.AnyAsync())
                {
                    await context.Suppliers.AddRangeAsync(
                        new Supplier { SuppName = "MedSupply Co.", SuppPhone = "9123456780" },
                        new Supplier { SuppName = "PharmaDist Pvt Ltd", SuppPhone = "9123456781" }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Suppliers seeded.");
                }

                logger.LogInformation("Database seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database seeding failed.");
                throw;
            }
        }
    }
}
