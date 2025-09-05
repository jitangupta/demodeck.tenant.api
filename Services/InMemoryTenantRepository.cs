namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly List<GlobalTenant> _tenants;

        public InMemoryTenantRepository()
        {
            _tenants = new List<GlobalTenant>
            {
                new GlobalTenant
                {
                    TenantId = "tnt_001",
                    TenantName = "acme",
                    DisplayName = "Acme Corporation",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    PrimaryRegion = "us-east",
                    AllowedRegions = new List<string> { "us-east", "us-west" },
                    CurrentVersion = "2.1",
                    Tier = TenantTier.Enterprise,
                    CustomConfig = new Dictionary<string, string>
                    {
                        ["max_users"] = "1000",
                        ["storage_gb"] = "500",
                        ["feature_analytics"] = "true"
                    },
                    Deployments = new List<TenantDeployment>
                    {
                        new TenantDeployment
                        {
                            Region = "us-east",
                            Version = "2.1",
                            ServiceName = "acme-v2-1-service",
                            HealthStatus = "Healthy",
                            LastHealthCheck = DateTime.UtcNow.AddMinutes(-2),
                            TrafficPercentage = 100
                        }
                    }
                },
                new GlobalTenant
                {
                    TenantId = "tnt_002",
                    TenantName = "globex",
                    DisplayName = "Globex Industries",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    PrimaryRegion = "us-west",
                    AllowedRegions = new List<string> { "us-west", "eu-west" },
                    CurrentVersion = "1.5",
                    Tier = TenantTier.Standard,
                    CustomConfig = new Dictionary<string, string>
                    {
                        ["max_users"] = "500",
                        ["storage_gb"] = "100",
                        ["feature_analytics"] = "false"
                    },
                    Deployments = new List<TenantDeployment>
                    {
                        new TenantDeployment
                        {
                            Region = "us-west",
                            Version = "1.5",
                            ServiceName = "globex-v1-5-service",
                            HealthStatus = "Healthy",
                            LastHealthCheck = DateTime.UtcNow.AddMinutes(-1),
                            TrafficPercentage = 80
                        },
                        new TenantDeployment
                        {
                            Region = "us-west",
                            Version = "2.0",
                            ServiceName = "globex-v2-0-service",
                            HealthStatus = "Healthy",
                            LastHealthCheck = DateTime.UtcNow.AddMinutes(-1),
                            TrafficPercentage = 20 // Canary deployment
                        }
                    }
                },
                new GlobalTenant
                {
                    TenantId = "tnt_003",
                    TenantName = "initech",
                    DisplayName = "Initech LLC",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    PrimaryRegion = "eu-west",
                    AllowedRegions = new List<string> { "eu-west" },
                    CurrentVersion = "1.0",
                    Tier = TenantTier.Basic,
                    CustomConfig = new Dictionary<string, string>
                    {
                        ["max_users"] = "100",
                        ["storage_gb"] = "25",
                        ["feature_analytics"] = "false"
                    },
                    Deployments = new List<TenantDeployment>
                    {
                        new TenantDeployment
                        {
                            Region = "eu-west",
                            Version = "1.0",
                            ServiceName = "initech-v1-0-service",
                            HealthStatus = "Degraded",
                            LastHealthCheck = DateTime.UtcNow.AddMinutes(-10),
                            TrafficPercentage = 100
                        }
                    }
                }
            };
        }

        public Task<GlobalTenant?> GetTenantByIdAsync(string tenantId)
        {
            var tenant = _tenants.FirstOrDefault(t => t.TenantId == tenantId);
            return Task.FromResult(tenant);
        }

        public Task<GlobalTenant?> GetTenantByNameAsync(string tenantName)
        {
            var tenant = _tenants.FirstOrDefault(t =>
                t.TenantName.Equals(tenantName, StringComparison.OrdinalIgnoreCase) && t.IsActive);
            return Task.FromResult(tenant);
        }

        public Task<List<GlobalTenant>> GetAllTenantsAsync()
        {
            return Task.FromResult(_tenants.Where(t => t.IsActive).ToList());
        }

        public Task<List<GlobalTenant>> GetTenantsByRegionAsync(string region)
        {
            var tenants = _tenants.Where(t =>
                t.IsActive &&
                t.Deployments.Any(d => d.Region.Equals(region, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return Task.FromResult(tenants);
        }

        public Task<bool> CreateTenantAsync(GlobalTenant tenant)
        {
            _tenants.Add(tenant);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateTenantAsync(GlobalTenant tenant)
        {
            var existingIndex = _tenants.FindIndex(t => t.TenantId == tenant.TenantId);
            if (existingIndex == -1) return Task.FromResult(false);

            _tenants[existingIndex] = tenant;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteTenantAsync(string tenantId)
        {
            var tenant = _tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (tenant == null) return Task.FromResult(false);

            tenant.IsActive = false;
            return Task.FromResult(true);
        }
    }
}
