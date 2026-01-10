using System;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework
{
    public class Vente
    {
        [Key]
        public int Id { get; set; }

        // Number of liters sold in this sale
        public int NbrLitres { get; set; }

        // Unit price for this sale (per liter)
        public decimal Prix { get; set; }

        // Persisted amount = NbrLitres * Prix (computed server-side before Save)
        public decimal Montant { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}