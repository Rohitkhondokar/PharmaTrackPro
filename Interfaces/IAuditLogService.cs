using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IAuditLogService
    {
        /// <param name="userIdOverride">Use when the acting user isn't yet reflected in HttpContext.User for this request — e.g. right after a successful login, before the auth cookie takes effect on the next request.</param>
        Task LogAsync(string action, string entityType, string? entityId, string details, string? userIdOverride = null, string? userNameOverride = null);

        Task<IEnumerable<AuditLog>> GetAllAsync(
            string? action = null,
            string? entityType = null,
            DateTime? from = null,
            DateTime? to = null);
    }
}
