using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    // DB columns (from screenshot):
    //   SaleDetId  int PK
    //   SaleId     int FK
    //   MedId      int FK
    //   Qty        decimal
    //   Rate       decimal
    //   Amt        decimal
    //   GstPct     decimal   ← percentage stored in DB
    //   GstAmt     decimal   ← computed GST amount stored in DB
    //   Total      decimal
    //
    // NOTE: The old entity had a property named "Gst" which did NOT match the DB column
    //       "GstAmt". AutoMapper was mapping Gst → GstAmt silently or failing.
    //       This file uses GstAmt everywhere; the VM still exposes a Gst alias.

    [Table("SaleDetails")]          // exact table name as shown in SSMS
    public class SaleDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleDetId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Required]
        [Display(Name = "Medicine")]
        public int MedId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Qty")]
        public decimal Qty { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount")]
        public decimal Amt { get; set; } = 0;

        // GST percentage (e.g. 5.00)
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "GST %")]
        public decimal GstPct { get; set; } = 0;

        // GST amount computed = Amt * GstPct / 100
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "GST Amount")]
        public decimal GstAmt { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; } = 0;

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey(nameof(SaleId))]
        public SaleMaster? SaleMaster { get; set; }

        [ForeignKey(nameof(MedId))]
        public MedicineMaster? Medicine { get; set; }
    }
}
