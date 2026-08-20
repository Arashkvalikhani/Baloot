using Balut.Application.Interfaces;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Balut.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string? details = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new AuditLog
            {
                UserId = httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}