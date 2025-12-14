using System.ComponentModel.DataAnnotations;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class Users
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string login_user { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [StringLength(10, ErrorMessage = "Employee ID cannot exceed 10 characters")]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Join Date")]
        public DateTime? JoinDate { get; set; }

        [Display(Name = "Profile Picture")]
        public string Photo { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}