using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A permanent record of a security- or business-significant action.
    /// UserName is a denormalized snapshot (not just a UserId FK) so the
    /// trail stays readable even if the user account is later renamed —
    /// audit records should never depend on current-state lookups.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string UserName { get; set; } = "System";

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? EntityId { get; set; }

        [StringLength(1000)]
        public string? Details { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
