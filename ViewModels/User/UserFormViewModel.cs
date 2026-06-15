using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.User
{
    public class UserFormViewModel
    {
        // Null/empty when creating a new user
        public string? Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select a role.")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Only required when creating (Id is null) — enforced in the service, not via DataAnnotations,
        // since the same view model serves both Create and Edit.
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
