namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class TenantHealthService : ITenantHealthService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IRegionRepository _regionRepository;

        public TenantHealthService(ITenantRepository tenantRepository, IRegionRepository regionRepository)
        {
            _tenantRepository = tenantRepository;
            _regionRepository = regionRepository;
        }

        public async Task<Dictionary<string, object>> GetTenantHealthAsync(string tenantId, string region)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                return new Dictionary<string, object>
                {
                    ["status"] = "NotFound",
                    ["message"] = "Tenant not found"
                };
            }

            var deployment = tenant.Deployments.FirstOrDefault(d => d.Region == region);
            if (deployment == null)
            {
                return new Dictionary<string, object>
                {
                    ["status"] = "NotDeployed",
                    ["message"] = $"Tenant not deployed in region {region}"
                };
            }

            return new Dictionary<string, object>
            {
                ["status"] = deployment.HealthStatus,
                ["version"] = deployment.Version,
                ["serviceName"] = deployment.ServiceName,
                ["lastHealthCheck"] = deployment.LastHealthCheck,
                ["trafficPercentage"] = deployment.TrafficPercentage,
                ["metrics"] = deployment.Metrics
            };
        }

        public async Task<Dictionary<string, object>> GetRegionHealthAsync(string region)
        {
            var regionInfo = await _regionRepository.GetRegionByIdAsync(region);
            if (regionInfo == null)
            {
                return new Dictionary<string, object>
                {
                    ["status"] = "NotFound",
                    ["message"] = "Region not found"
                };
            }

            var tenants = await _tenantRepository.GetTenantsByRegionAsync(region);
            var healthyTenants = tenants.Count(t =>
                t.Deployments.Any(d => d.Region == region && d.HealthStatus == "Healthy"));

            return new Dictionary<string, object>
            {
                ["status"] = regionInfo.IsActive ? "Active" : "Inactive",
                ["totalTenants"] = tenants.Count,
                ["healthyTenants"] = healthyTenants,
                ["unhealthyTenants"] = tenants.Count - healthyTenants,
                ["currentLoad"] = regionInfo.CurrentLoad,
                ["availableVersions"] = regionInfo.AvailableVersions,
                ["metrics"] = regionInfo.RegionMetrics
            };
        }

        public async Task UpdateTenantHealthAsync(string tenantId, string region, Dictionary<string, object> healthData)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null) return;

            var deployment = tenant.Deployments.FirstOrDefault(d => d.Region == region);
            if (deployment == null) return;

            if (healthData.ContainsKey("status"))
            {
                deployment.HealthStatus = healthData["status"]?.ToString() ?? "Unknown";
            }

            deployment.LastHealthCheck = DateTime.UtcNow;

            if (healthData.ContainsKey("metrics") && healthData["metrics"] is Dictionary<string, object> metrics)
            {
                deployment.Metrics = metrics;
            }

            await _tenantRepository.UpdateTenantAsync(tenant);
        }
    }
}
