namespace Medical_Store_Billing_System.Models
{
    public class BaseEntity
    {

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
