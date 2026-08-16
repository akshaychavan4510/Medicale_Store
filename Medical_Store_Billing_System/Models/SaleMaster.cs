using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    // DB columns (from screenshot):
    //   SaleId        int PK
    //   SaleDate      date
    //   CustId        int FK
    //   GrandTotal    decimal
    //   Discount      decimal   ← was MISSING from old model – caused EF mapping error
    //   NetTotal      decimal   ← was MISSING from old model – caused EF mapping error
    //   Note          nvarchar
    //   CreatedBy     nvarchar
    //   CreatedDate   datetime2
    //   ModifiedDate  datetime2

    [Table("SaleMaster")]           // match exact table name in your DB (no underscore)
    public class SaleMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleId { get; set; }

        [Column(TypeName = "date")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public int CustId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; } = 0;

        // ✅ Added – DB has this column; leaving it out caused SqlException on INSERT
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;

        // ✅ Added – DB has this column
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTotal { get; set; } = 0;

        public string? Note { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<SaleDetails> SaleDetails { get; set; } = new List<SaleDetails>();
    }
}
