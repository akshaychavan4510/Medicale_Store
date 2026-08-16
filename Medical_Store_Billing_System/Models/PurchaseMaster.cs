// Medical_Store_Billing_System/Models/PurchaseMaster.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    [Table("PurchaseMaster")]
    public class PurchaseMaster
    {
        [Key]
        [Column("PurchaseId")]
        public int PurchaseId { get; set; }

        [Column("PurchaseDate")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Column("SuppId")]
        public int SuppId { get; set; }

        [Column("InvoiceNo")]
        public string? InvoiceNo { get; set; }

        [Column("GrandTotal")]
        public decimal GrandTotal { get; set; }

        [Column("Discount")]
        public decimal Discount { get; set; }

        [Column("NetTotal")]
        public decimal NetTotal { get; set; }

        [Column("Note")]
        public string? Note { get; set; }

        [Column("CreatedBy")]
        public string? CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey("SuppId")]
        public virtual Supplier? Supplier { get; set; }

        public virtual ICollection<PurchaseDetails> PurchaseDetails { get; set; } = new List<PurchaseDetails>();
    }
}