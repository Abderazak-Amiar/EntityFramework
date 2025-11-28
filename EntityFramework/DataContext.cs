namespace EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        public DbSet<User>? Users { get; set; }

        // Add Parameters DbSet so application settings can be persisted
        public DbSet<Parameters>? Parameters { get; set; }

        // New: Ventes table
        public DbSet<Vente>? Ventes { get; set; }

        // call this from SaveChanges / SaveChangesAsync to persist formatted strings

        private void UpdateDisplayFields()
        {
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            foreach (var entry in ChangeTracker.Entries<User>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var u = entry.Entity;

                u.DisplayNbrBags = u.NbrBags != 0m ? FormatDecimalSmart(u.NbrBags, fr) : null;
                u.DisplayWeight = (u.Weight.HasValue && u.Weight.Value != 0m) ? FormatDecimalSmart(u.Weight.Value, fr) : null;
                u.DisplayUnitPrice = (u.UnitPriceLiter.HasValue && u.UnitPriceLiter.Value != 0m) ? FormatDecimalSmart(u.UnitPriceLiter.Value, fr) : null;
                u.DisplayAmountDue = (u.AmountDue.HasValue && u.AmountDue.Value != 0m) ? FormatDecimalSmart(u.AmountDue.Value, fr) : null;
            }

            // Ensure ventes have persisted Montant computed
            foreach (var vEntry in ChangeTracker.Entries<Vente>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var v = vEntry.Entity;
                v.Montant = v.NbrLitres * v.Prix;
            }
        }

        private static string FormatDecimalSmart(decimal value, CultureInfo ci)
        {
            return decimal.Truncate(value) == value
                ? value.ToString("N0", ci)
                : value.ToString("N1", ci);
        }

        public override int SaveChanges()
        {
            UpdateDisplayFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateDisplayFields();
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
