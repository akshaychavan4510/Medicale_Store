using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    [Table("PurchaseDetails")]
    public class PurchaseDetails
    {
        [Key]
        [Column("PurchaseDetId")]
        public int PurchaseDetId { get; set; }

        [Column("PurchaseId")]
        public int PurchaseId { get; set; }

        [Column("MedId")]
        public int MedId { get; set; }

        [Column("Qty")]
        public decimal Qty { get; set; }

        [Column("Rate")]
        public decimal Rate { get; set; }

        [Column("Amt")]
        public decimal Amt { get; set; }

        [Column("GstPct")]
        public decimal GstPct { get; set; }

        [Column("GstAmt")]
        public decimal GstAmt { get; set; }

        [Column("Total")]
        public decimal Total { get; set; }

        [Column("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("BatchNo")]
        public string? BatchNo { get; set; }

        // Navigation properties — FK relationships handled in PurchaseDetailsConfig (Fluent API)
        public virtual PurchaseMaster? PurchaseMaster { get; set; }

        public virtual MedicineMaster? Medicine { get; set; }
    }
}