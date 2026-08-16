using MedicalStore.Business.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    [Table("Customer")]
    public class Customer
    {
        [Key]
        public int CustId { get; set; }

        [Required]
        [StringLength(150)]
        public string CustName { get; set; } = string.Empty;

        [StringLength(15)]
        public string? CustPhone { get; set; }

        [StringLength(100)]
        public string? CustEmail { get; set; }

        [StringLength(250)]
        public string? CustAddress { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CustBal { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<SaleMaster> Sales { get; set; } = new List<SaleMaster>();
        public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}