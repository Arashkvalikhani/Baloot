namespace Balut.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityType, string? entityId, string? details = null);
    }
}