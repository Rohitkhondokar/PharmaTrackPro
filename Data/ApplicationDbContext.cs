using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Models;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Data
{
    /// <summary>
    /// Core EF Core context for PharmaTrack Pro.
    /// Extends IdentityDbContext so Identity tables (Users, Roles, Claims, etc.)
    /// are created alongside our business tables.
    ///
    /// Business entity DbSets (Medicines, Categories, Purchases, Sales, etc.)
    /// will be added incrementally as each module is built, each with its own
    /// migration so the history stays readable and reviewable.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Medicine> Medicines => Set<Medicine>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Batch> Batches => Set<Batch>();
        public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
        public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
        public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<PharmacySettings> PharmacySettings => Set<PharmacySettings>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename default Identity tables to a cleaner, project-consistent naming scheme.
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Global query filter for soft-deleted users
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);

            // Category configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.HasIndex(c => c.Name); // not unique at DB level — uniqueness among active rows is enforced in the service layer
            });

            // Manufacturer configuration
            builder.Entity<Manufacturer>(entity =>
            {
                entity.HasQueryFilter(m => !m.IsDeleted);
                entity.HasIndex(m => m.Name);
            });

            // Medicine configuration
            builder.Entity<Medicine>(entity =>
            {
                entity.HasQueryFilter(m => !m.IsDeleted);
                entity.HasIndex(m => new { m.Name, m.Strength });

                entity.HasOne(m => m.Category)
                      .WithMany(c => c.Medicines)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Manufacturer)
                      .WithMany(mf => mf.Medicines)
                      .HasForeignKey(m => m.ManufacturerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Supplier configuration
            builder.Entity<Supplier>(entity =>
            {
                entity.HasQueryFilter(s => !s.IsDeleted);
                entity.HasIndex(s => s.Name);
            });

            // Purchase configuration (no soft delete — financial/audit record)
            builder.Entity<Purchase>(entity =>
            {
                entity.HasOne(p => p.Supplier)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(p => p.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(p => p.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseItem>(entity =>
            {
                entity.HasOne(i => i.Purchase)
                      .WithMany(p => p.Items)
                      .HasForeignKey(i => i.PurchaseId)
                      .OnDelete(DeleteBehavior.Cascade); // deleting a purchase (if ever) removes its lines

                entity.HasOne(i => i.Medicine)
                      .WithMany()
                      .HasForeignKey(i => i.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Batch>(entity =>
            {
                entity.HasOne(b => b.Medicine)
                      .WithMany(m => m.Batches)
                      .HasForeignKey(b => b.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.PurchaseItem)
                      .WithMany()
                      .HasForeignKey(b => b.PurchaseItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseReturn configuration (no soft delete — audit record)
            builder.Entity<PurchaseReturn>(entity =>
            {
                entity.HasOne(r => r.Purchase)
                      .WithMany(p => p.Returns)
                      .HasForeignKey(r => r.PurchaseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(r => r.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseReturnItem>(entity =>
            {
                entity.HasOne(i => i.PurchaseReturn)
                      .WithMany(r => r.Items)
                      .HasForeignKey(i => i.PurchaseReturnId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Batch)
                      .WithMany()
                      .HasForeignKey(i => i.BatchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.HasIndex(c => c.Phone);
            });

            // Sale configuration (no soft delete — financial record)
            builder.Entity<Sale>(entity =>
            {
                entity.HasOne(s => s.Customer)
                      .WithMany(c => c.Sales)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.CashierUser)
                      .WithMany()
                      .HasForeignKey(s => s.CashierUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SaleItem>(entity =>
            {
                entity.HasOne(i => i.Sale)
                      .WithMany(s => s.Items)
                      .HasForeignKey(i => i.SaleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Medicine)
                      .WithMany()
                      .HasForeignKey(i => i.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Batch)
                      .WithMany()
                      .HasForeignKey(i => i.BatchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SaleReturn configuration (no soft delete — audit record)
            builder.Entity<SaleReturn>(entity =>
            {
                entity.HasOne(r => r.Sale)
                      .WithMany(s => s.Returns)
                      .HasForeignKey(r => r.SaleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(r => r.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SaleReturnItem>(entity =>
            {
                entity.HasOne(i => i.SaleReturn)
                      .WithMany(r => r.Items)
                      .HasForeignKey(i => i.SaleReturnId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.SaleItem)
                      .WithMany()
                      .HasForeignKey(i => i.SaleItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.ReferenceKey).IsUnique();
            });
        }
    }
}

