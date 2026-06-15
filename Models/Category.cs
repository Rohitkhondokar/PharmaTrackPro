using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// Medicine category (e.g. Analgesics, Antibiotics, Antihistamines).
    /// Referenced by Medicine once the Medicine Management module is built.
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

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
