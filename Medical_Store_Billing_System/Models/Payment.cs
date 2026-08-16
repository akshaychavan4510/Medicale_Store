using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Supplier is required.")]
        [Display(Name = "Supplier")]
        public int SuppId { get; set; }

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
        [ForeignKey(nameof(SuppId))]
        public virtual Supplier? Supplier { get; set; }
    }
}