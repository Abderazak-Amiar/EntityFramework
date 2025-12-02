using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace EntityFramework
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string Name { get; set; }

        [Required, MaxLength(50)]
        public required string Phone { get; set; }

        [Required, MaxLength(500)]
        public required string Address { get; set; }

        public required decimal NbrBags { get; set; }

        [Required, MaxLength(100)]
        public required string NbrContainers { get; set; }

        public decimal? Weight { get; set; }
        public int? NbrLiters { get; set; }
        public decimal? UnitPriceLiter { get; set; }
        public int? PayedLiters { get; set; }
        public decimal? AmountDue { get; set; }

        // Persisted formatted columns (optional). Use migrations to add these to DB.
        [MaxLength(50)]
        public string? DisplayNbrBags { get; set; }

        [MaxLength(50)]
        public string? DisplayWeight { get; set; }

        [MaxLength(50)]
        public string? DisplayUnitPrice { get; set; }

        [MaxLength(50)]
        public string? DisplayAmountDue { get; set; }

        // NEW: persisted formatted rendement (litres per 100kg). Use migrations to add this column.
        [MaxLength(50)]
        public string? DisplayRendement { get; set; }

        // Convenience computed properties (not stored)
        [NotMapped]
        public string? NbrBagsFormatted => FormatDecimalSmart(NbrBags);

        [NotMapped]
        public string? WeightFormatted => Weight.HasValue ? FormatDecimalSmart(Weight.Value) : null;

        private static string FormatDecimalSmart(decimal value)
        {
            var fr = CultureInfo.GetCultureInfo("fr-FR");
            return decimal.Truncate(value) == value
                ? value.ToString("N0", fr)
                : value.ToString("N1", fr);
        }

        public DateTime? CreatedAt { get; set; }

        // NEW: persisted mode flag (true = Portion, false = Paiement). Nullable for migration safety.
        public bool? IsPortionMode { get; set; }
    }
}
