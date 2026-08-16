using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Store_Billing_System.Models
{
    public class MedicineMaster
    {
        public int MedId { get; set; }

        public string MedName { get; set; } = string.Empty;

        public int CatId { get; set; }

        public int BrandId { get; set; }

        public string? Unit { get; set; }

        public decimal PurchaseRate { get; set; }

        public decimal SaleRate { get; set; }

        public decimal GstPct { get; set; }

        public decimal Stock { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string? BatchNo { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual MedicineCategory Category { get; set; }

        public virtual Brand Brand { get; set; }

        public virtual ICollection<PurchaseDetails> PurchaseDetails { get; set; }
            = new List<PurchaseDetails>();

        public virtual ICollection<SaleDetails> SaleDetails { get; set; }
            = new List<SaleDetails>();
    }
}
