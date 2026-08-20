namespace Balut.Application.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserIdRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class ChangeUserRolesRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class ResetPasswordRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}