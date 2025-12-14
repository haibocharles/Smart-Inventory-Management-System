using System.ComponentModel.DataAnnotations;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class Change_Password
    {
        [Required (ErrorMessage ="Current password is required")]
        [DataType(DataType.Password)]// 指定輸入類型為密碼
        [Display(Name = "Current Password")]
        public string CurrentPassword {get; set; }


        [Required(ErrorMessage = "Please input the New Password ")]
        [DataType(DataType.Password)]// 指定輸入類型為密碼
        [StringLength(100, ErrorMessage = "Password must have at least 6 characters", MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [CompareAttribute("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


    }
}
