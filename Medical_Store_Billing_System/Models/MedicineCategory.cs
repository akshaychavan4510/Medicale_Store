using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    [Table("MedicineCategory")]
    public class MedicineCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CatId { get; set; }

        [Required, StringLength(100)]
        public string CatName { get; set; } = string.Empty;

        // ✅ FIX: Description column exists in DB — must be in model
        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public virtual ICollection<MedicineMaster> Medicines { get; set; }
            = new List<MedicineMaster>();
    }
}