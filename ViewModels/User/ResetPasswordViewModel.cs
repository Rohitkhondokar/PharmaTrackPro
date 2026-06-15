using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.User
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a new password.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
