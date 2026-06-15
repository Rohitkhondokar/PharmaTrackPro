using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;

namespace PharmaTrackPro.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? action, string? entityType, DateTime? from, DateTime? to)
        {
            var logs = await _auditLogService.GetAllAsync(action, entityType, from, to);

            var data = logs.Select(l => new
            {
                l.Id,
                l.UserName,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.Details,
                l.IpAddress,
                CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(new { data });
        }
    }
}
