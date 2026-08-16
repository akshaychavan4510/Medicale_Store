using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Medical_Store_Billing_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}