using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.Customer
{
    public class CustomerFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters.")]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        [Display(Name = "Medical Notes")]
        public string? MedicalNotes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
