using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EntityFramework
{
    // Lightweight DTO + factory to compute stats from your user list or DbContext.
    public class Statistics
    {
        public int TotalUsers { get; set; }
        public decimal TotalNbrBags { get; set; }
        public decimal TotalWeight { get; set; }
        public int TotalLiters { get; set; }
        public decimal TotalAmountDue { get; set; }
        public decimal AverageUnitPrice { get; set; }

        // Optional: top N debtors (Id, Name, AmountDue) - not stored, just a helper view.
        public IReadOnlyList<(int Id, string Name, decimal AmountDue)> TopDebtors { get; init; } = Array.Empty<(int, string, decimal)>();

        public static Statistics FromUsers(IEnumerable<User>? users)
        {
            var list = (users ?? Array.Empty<User>()).ToList();

            var totalUsers = list.Count;
            var totalBags = list.Sum(u => u.NbrBags);
            var totalWeight = list.Sum(u => u.Weight ?? 0m);
            var totalLiters = list.Sum(u => u.NbrLiters ?? 0);
            var totalAmountDue = list.Sum(u => u.AmountDue ?? 0m);

            var unitPrices = list.Where(u => u.UnitPriceLiter.HasValue).Select(u => u.UnitPriceLiter!.Value).ToArray();
            var avgUnitPrice = unitPrices.Length > 0 ? unitPrices.Average() : 0m;

            var topDebtors = list
                .Where(u => (u.AmountDue ?? 0m) > 0m)
                .OrderByDescending(u => u.AmountDue)
                .Take(5)
                .Select(u => (u.Id, u.Name ?? string.Empty, u.AmountDue ?? 0m))
                .ToList();

            return new Statistics
            {
                TotalUsers = totalUsers,
                TotalNbrBags = totalBags,
                TotalWeight = totalWeight,
                TotalLiters = totalLiters,
                TotalAmountDue = totalAmountDue,
                AverageUnitPrice = avgUnitPrice,
                TopDebtors = topDebtors
            };
        }

        public static Statistics FromContext(DataContext context)
        {
            if (context == null) return new Statistics();
            var users = context.Users?.ToList() ?? new List<User>();
            return FromUsers(users);
        }

        public string ToSummaryString(CultureInfo? ci = null)
        {
            ci ??= CultureInfo.CurrentCulture;
            // Use compact formatting that is readable in the UI
            return
                $"Clients: {TotalUsers}, " +
                $"Sacs: {TotalNbrBags.ToString("N1", ci)}, " +
                $"Poids: {TotalWeight.ToString("N1", ci)}, " +
                $"Litres: {TotalLiters:N0}, " +
                $"Montant dû: {TotalAmountDue.ToString("N2", ci)}, " +
                $"Prix moyen: {AverageUnitPrice.ToString("N2", ci)}";
        }
    }
}