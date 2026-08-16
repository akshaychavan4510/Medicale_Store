// Models/Supplier.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SuppId { get; set; }

        [Required, StringLength(150)]
        public string SuppName { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SuppPhone { get; set; }

        [StringLength(100)]
        public string? SuppEmail { get; set; }

        [StringLength(250)]
        public string? SuppAddress { get; set; }

        [StringLength(50)]
        public string? GstNo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SuppBal { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public virtual ICollection<PurchaseMaster> PurchaseMasters { get; set; } = new List<PurchaseMaster>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}