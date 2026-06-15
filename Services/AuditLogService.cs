using System.Security.Claims;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IGenericRepository<AuditLog> _auditLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IGenericRepository<AuditLog> auditLogRepository, IHttpContextAccessor httpContextAccessor)
        {
            _auditLogRepository = auditLogRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string details, string? userIdOverride = null, string? userNameOverride = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = userIdOverride ?? httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = userNameOverride ?? httpContext?.User?.Identity?.Name;

            var log = new AuditLog
            {
                UserId = userId,
                UserName = string.IsNullOrEmpty(userName) ? "System" : userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(log);
            await _auditLogRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(
            string? action = null,
            string? entityType = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var logs = (await _auditLogRepository.GetAllAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(action))
            {
                logs = logs.Where(l => l.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                logs = logs.Where(l => l.EntityType == entityType);
            }

            if (from.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date <= to.Value.Date);
            }

            return logs.OrderByDescending(l => l.CreatedAt).Take(500).ToList();
        }
    }
}
