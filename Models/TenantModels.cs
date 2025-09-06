namespace Demodeck.Tenant.Api.Models
{
    // Simplified Tenant Model with embedded YARP routing info
    public class TenantDto
    {
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PrimaryRegion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;

        // YARP routing info (embedded in tenant response)
        public string ServiceName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; } = true;
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }

    // Release Management Model
    public class Release
    {
        public string Id { get; set; } = string.Empty;
        public string ReleaseName { get; set; } = string.Empty;
        public string ReleaseVersion { get; set; } = string.Empty;
        public string AuthApiVersion { get; set; } = string.Empty;
        public string ProductApiVersion { get; set; } = string.Empty;
        public string UiAppVersion { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsRolledBack { get; set; } = false;
    }

    // Manager Model for Authentication
    public class Manager
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    // API Response Models
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public Manager Manager { get; set; }
    }

    // JWT Settings
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenLifetimeMinutes { get; set; } = 60;
    }
}