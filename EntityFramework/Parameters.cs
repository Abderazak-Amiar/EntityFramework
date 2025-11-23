using System;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework
{
    // Simple EF entity to store application settings.
    // Persist it (single-row) via migrations if you want settings to survive restarts.
    public class Parameters
    {
        [Key]
        public int Id { get; set; } = 1; // default single-row id

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        // Combined company address / phone (per UI)
        [MaxLength(500)]
        public string? CompanyAddressPhone { get; set; }

        // Prix/L (mapped from UI txtPricePerLiter)
        public decimal DefaultUnitPrice { get; set; } = 0m;

        // Portion (mapped from UI txtPortion)
        public decimal DefaultPortion { get; set; } = 0m;

        public DateTime? UpdatedAt { get; set; }

        public void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}