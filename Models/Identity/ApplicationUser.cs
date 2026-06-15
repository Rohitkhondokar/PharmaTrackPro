using Microsoft.AspNetCore.Identity;

namespace PharmaTrackPro.Models.Identity
{
    /// <summary>
    /// Extends the default IdentityUser with fields needed for pharmacy staff records.
    /// Keeping this lean now; additional profile fields can be added via migration
    /// as later modules (User Management) require them.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? ProfileImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Soft delete support, consistent with the rest of the system
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
