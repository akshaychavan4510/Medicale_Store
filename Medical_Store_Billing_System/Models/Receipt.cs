using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    public class Receipt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReceiptId { get; set; }

        [Required]
        [Display(Name = "Receipt Date")]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount (₹)")]
        public decimal Amount { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Payment Mode")]
        public string? PayMode { get; set; } = "Cash";

        [StringLength(100)]
        [Display(Name = "Reference No.")]
        public string? RefNo { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }
    }
}
