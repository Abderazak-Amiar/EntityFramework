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
            var fr = CultureInfo.InvariantCulture; // use '.' as decimal separator

            // Read DefaultPortion (once) from Parameters and normalize to fraction [0..1]
            decimal defaultPortionFraction =0m;
            try
            {
                var p = Parameters?.FirstOrDefault(pt => pt.Id ==1);
                if (p != null)
                {
                    defaultPortionFraction = p.DefaultPortion;
                    if (defaultPortionFraction >1m)
                        defaultPortionFraction /=100m; // treat as percent

                    if (defaultPortionFraction <0m) defaultPortionFraction =0m;
                    if (defaultPortionFraction >1m) defaultPortionFraction =1m;
                }
            }
            catch
            {
                defaultPortionFraction =0m;
            }

            foreach (var entry in ChangeTracker.Entries<User>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var u = entry.Entity;

                u.DisplayNbrBags = u.NbrBags !=0m ? FormatDecimalSmart(u.NbrBags, fr) : null;
                u.DisplayWeight = (u.Weight.HasValue && u.Weight.Value !=0m) ? FormatDecimalSmart(u.Weight.Value, fr) : null;
                u.DisplayUnitPrice = (u.UnitPriceLiter.HasValue && u.UnitPriceLiter.Value !=0m) ? FormatDecimalSmart(u.UnitPriceLiter.Value, fr) : null;
                u.DisplayAmountDue = (u.AmountDue.HasValue && u.AmountDue.Value !=0m) ? FormatDecimalSmart(u.AmountDue.Value, fr) : null;

                // compute and persist rendement (litres per100kg) if possible
                try
                {
                    if (u.Weight.HasValue && u.Weight.Value !=0m && u.NbrLiters.HasValue && u.NbrLiters.Value !=0)
                    {
                        var litres = (decimal)u.NbrLiters.Value;
                        var poids = u.Weight.Value;
                        var rendement = (litres *100m) / poids;
                        u.DisplayRendement = FormatDecimalSmart(rendement, fr);
                    }
                    else
                    {
                        u.DisplayRendement = null;
                    }
                }
                catch
                {
                    u.DisplayRendement = null;
                }

                // compute and persist PortionLiters and DeliveredLiters and their display strings when possible
                try
                {
                    if (u.NbrLiters.HasValue && u.NbrLiters.Value !=0 && defaultPortionFraction >0m)
                    {
                        var totalLiters = (decimal)u.NbrLiters.Value;
                        var portionLiters = defaultPortionFraction * totalLiters;
                        var deliveredLiters = totalLiters - portionLiters;
                        if (deliveredLiters <0m) deliveredLiters =0m;

                        u.PortionLiters = portionLiters;
                        u.DeliveredLiters = deliveredLiters;

                        u.DisplayPortion = FormatDecimalSmart(portionLiters, fr);
                        u.DisplayDelivered = FormatDecimalSmart(deliveredLiters, fr);
                    }
                    else
                    {
                        u.PortionLiters = null;
                        u.DeliveredLiters = null;
                        u.DisplayPortion = null;
                        u.DisplayDelivered = null;
                    }
                }
                catch
                {
                    u.PortionLiters = null;
                    u.DeliveredLiters = null;
                    u.DisplayPortion = null;
                    u.DisplayDelivered = null;
                }
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
