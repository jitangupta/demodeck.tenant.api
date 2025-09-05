namespace Demodeck.Tenant.Api.Models
{
    public class GlobalTenant
    {
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty; // URL-friendly name (acme, globex)
        public string DisplayName { get; set; } = string.Empty; // Human-readable (Acme Corp)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string PrimaryRegion { get; set; } = string.Empty;
        public List<string> AllowedRegions { get; set; } = new();
        public string CurrentVersion { get; set; } = "1.0";
        public TenantTier Tier { get; set; } = TenantTier.Standard;
        public Dictionary<string, string> CustomConfig { get; set; } = new();
        public List<TenantDeployment> Deployments { get; set; } = new();
    }

    public class TenantDeployment
    {
        public string Region { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty; // K8s service name
        public string HealthStatus { get; set; } = "Unknown";
        public DateTime LastHealthCheck { get; set; }
        public int TrafficPercentage { get; set; } = 100; // For canary deployments
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    public class Region
    {
        public string RegionId { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ApiGatewayUrl { get; set; } = string.Empty;
        public List<string> AvailableVersions { get; set; } = new();
        public int CurrentLoad { get; set; } = 0; // Number of tenants
        public Dictionary<string, object> RegionMetrics { get; set; } = new();
    }

    public class TenantRoutingInfo
    {
        public string TenantName { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string PrimaryRegion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; } = true;
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }

    public class YarpConfiguration
    {
        public Dictionary<string, YarpRoute> Routes { get; set; } = new();
        public Dictionary<string, YarpCluster> Clusters { get; set; } = new();
    }

    public class YarpRoute
    {
        public string ClusterId { get; set; } = string.Empty;
        public YarpMatch Match { get; set; } = new();
        public List<Dictionary<string, object>> Transforms { get; set; } = new();
    }

    public class YarpMatch
    {
        public List<string> Path { get; set; } = new();
        public List<string> Hosts { get; set; } = new();
    }

    public class YarpCluster
    {
        public Dictionary<string, YarpDestination> Destinations { get; set; } = new();
        public YarpHealthCheck? HealthCheck { get; set; }
    }

    public class YarpDestination
    {
        public string Address { get; set; } = string.Empty;
    }

    public class YarpHealthCheck
    {
        public YarpActiveHealthCheck Active { get; set; } = new();
    }

    public class YarpActiveHealthCheck
    {
        public bool Enabled { get; set; } = true;
        public string Interval { get; set; } = "00:00:30";
        public string Timeout { get; set; } = "00:00:05";
        public string Policy { get; set; } = "ConsecutiveFailures";
        public string Path { get; set; } = "/health";
    }

    public class CreateTenantRequest
    {
        public string TenantName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PrimaryRegion { get; set; } = string.Empty;
        public TenantTier Tier { get; set; } = TenantTier.Standard;
        public Dictionary<string, string> CustomConfig { get; set; } = new();
    }

    public class UpdateTenantVersionRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string TargetVersion { get; set; } = string.Empty;
        public int TrafficPercentage { get; set; } = 100;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }

    public enum TenantTier
    {
        Basic,
        Standard,
        Premium,
        Enterprise
    }
}
