namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IRegionRepository _regionRepository;

        public TenantService(ITenantRepository tenantRepository, IRegionRepository regionRepository)
        {
            _tenantRepository = tenantRepository;
            _regionRepository = regionRepository;
        }

        public async Task<TenantRoutingInfo?> GetTenantRoutingInfoAsync(string tenantName)
        {
            var tenant = await _tenantRepository.GetTenantByNameAsync(tenantName);
            if (tenant == null) return null;

            var primaryDeployment = tenant.Deployments.FirstOrDefault(d => d.Region == tenant.PrimaryRegion);
            if (primaryDeployment == null) return null;

            return new TenantRoutingInfo
            {
                TenantName = tenant.TenantName,
                TenantId = tenant.TenantId,
                PrimaryRegion = tenant.PrimaryRegion,
                CurrentVersion = primaryDeployment.Version,
                ServiceName = primaryDeployment.ServiceName,
                IsHealthy = primaryDeployment.HealthStatus == "Healthy",
                CustomHeaders = new Dictionary<string, string>
                {
                    ["X-Tenant-Id"] = tenant.TenantId,
                    ["X-Tenant-Version"] = primaryDeployment.Version,
                    ["X-Tenant-Tier"] = tenant.Tier.ToString()
                }
            };
        }

        public async Task<List<TenantRoutingInfo>> GetAllTenantRoutingInfoAsync(string? region = null)
        {
            var tenants = string.IsNullOrEmpty(region)
                ? await _tenantRepository.GetAllTenantsAsync()
                : await _tenantRepository.GetTenantsByRegionAsync(region);

            var routingInfos = new List<TenantRoutingInfo>();

            foreach (var tenant in tenants)
            {
                var deployment = string.IsNullOrEmpty(region)
                    ? tenant.Deployments.FirstOrDefault(d => d.Region == tenant.PrimaryRegion)
                    : tenant.Deployments.FirstOrDefault(d => d.Region == region);

                if (deployment != null)
                {
                    routingInfos.Add(new TenantRoutingInfo
                    {
                        TenantName = tenant.TenantName,
                        TenantId = tenant.TenantId,
                        PrimaryRegion = tenant.PrimaryRegion,
                        CurrentVersion = deployment.Version,
                        ServiceName = deployment.ServiceName,
                        IsHealthy = deployment.HealthStatus == "Healthy",
                        CustomHeaders = new Dictionary<string, string>
                        {
                            ["X-Tenant-Id"] = tenant.TenantId,
                            ["X-Tenant-Version"] = deployment.Version,
                            ["X-Tenant-Tier"] = tenant.Tier.ToString()
                        }
                    });
                }
            }

            return routingInfos;
        }

        public async Task<bool> UpdateTenantVersionAsync(UpdateTenantVersionRequest request)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(request.TenantId);
            if (tenant == null) return false;

            var deployment = tenant.Deployments.FirstOrDefault(d => d.Region == request.Region);
            if (deployment == null)
            {
                // Add new deployment for this region
                tenant.Deployments.Add(new TenantDeployment
                {
                    Region = request.Region,
                    Version = request.TargetVersion,
                    ServiceName = $"{tenant.TenantName}-v{request.TargetVersion.Replace(".", "-")}-service",
                    HealthStatus = "Unknown",
                    LastHealthCheck = DateTime.UtcNow,
                    TrafficPercentage = request.TrafficPercentage
                });
            }
            else
            {
                deployment.Version = request.TargetVersion;
                deployment.ServiceName = $"{tenant.TenantName}-v{request.TargetVersion.Replace(".", "-")}-service";
                deployment.TrafficPercentage = request.TrafficPercentage;
            }

            return await _tenantRepository.UpdateTenantAsync(tenant);
        }

        public async Task<GlobalTenant?> CreateTenantAsync(CreateTenantRequest request)
        {
            // Check if tenant name already exists
            var existing = await _tenantRepository.GetTenantByNameAsync(request.TenantName);
            if (existing != null) return null;

            var tenant = new GlobalTenant
            {
                TenantId = $"tnt_{Guid.NewGuid().ToString("N")[..8]}",
                TenantName = request.TenantName.ToLowerInvariant(),
                DisplayName = request.DisplayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PrimaryRegion = request.PrimaryRegion,
                AllowedRegions = new List<string> { request.PrimaryRegion },
                CurrentVersion = "1.0",
                Tier = request.Tier,
                CustomConfig = request.CustomConfig,
                Deployments = new List<TenantDeployment>
                {
                    new TenantDeployment
                    {
                        Region = request.PrimaryRegion,
                        Version = "1.0",
                        ServiceName = $"{request.TenantName.ToLowerInvariant()}-v1-0-service",
                        HealthStatus = "Provisioning",
                        LastHealthCheck = DateTime.UtcNow,
                        TrafficPercentage = 100
                    }
                }
            };

            var created = await _tenantRepository.CreateTenantAsync(tenant);
            return created ? tenant : null;
        }
    }
}
