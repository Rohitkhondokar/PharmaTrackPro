using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A medicine manufacturer / pharmaceutical company.
    /// Referenced by Medicine once the Medicine Management module is built.
    /// </summary>
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
