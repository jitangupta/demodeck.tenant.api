namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class YarpConfigService : IYarpConfigService
    {
        private readonly ITenantService _tenantService;
        private readonly IRegionRepository _regionRepository;

        public YarpConfigService(ITenantService tenantService, IRegionRepository regionRepository)
        {
            _tenantService = tenantService;
            _regionRepository = regionRepository;
        }

        public async Task<YarpConfiguration> GenerateYarpConfigAsync(string region)
        {
            var tenantRoutingInfos = await _tenantService.GetAllTenantRoutingInfoAsync(region);
            var config = new YarpConfiguration();

            foreach (var tenant in tenantRoutingInfos)
            {
                var routeId = $"route_{tenant.TenantName}";
                var clusterId = $"cluster_{tenant.TenantName}";

                // Create route
                config.Routes[routeId] = new YarpRoute
                {
                    ClusterId = clusterId,
                    Match = new YarpMatch
                    {
                        Path = new List<string> { $"/{tenant.TenantName}/{{**catch-all}}" }
                    },
                    Transforms = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["RequestHeader"] = new Dictionary<string, string>
                            {
                                ["X-Tenant-Id"] = tenant.TenantId,
                                ["X-Tenant-Version"] = tenant.CurrentVersion,
                                ["X-Tenant-Tier"] = tenant.CustomHeaders.GetValueOrDefault("X-Tenant-Tier", "Standard")
                            }
                        },
                        new Dictionary<string, object>
                        {
                            ["PathPattern"] = new Dictionary<string, string>
                            {
                                ["Pattern"] = "/{**catch-all}"
                            }
                        }
                    }
                };

                // Create cluster
                config.Clusters[clusterId] = new YarpCluster
                {
                    Destinations = new Dictionary<string, YarpDestination>
                    {
                        [$"destination_{tenant.TenantName}"] = new YarpDestination
                        {
                            Address = $"http://{tenant.ServiceName}/"
                        }
                    },
                    HealthCheck = new YarpHealthCheck
                    {
                        Active = new YarpActiveHealthCheck
                        {
                            Enabled = true,
                            Interval = "00:00:30",
                            Timeout = "00:00:05",
                            Policy = "ConsecutiveFailures",
                            Path = "/health"
                        }
                    }
                };
            }

            return config;
        }

        public async Task<YarpConfiguration> GenerateGlobalYarpConfigAsync()
        {
            var allTenantRoutingInfos = await _tenantService.GetAllTenantRoutingInfoAsync();
            var regions = await _regionRepository.GetAllActiveRegionsAsync();
            var config = new YarpConfiguration();

            foreach (var tenant in allTenantRoutingInfos)
            {
                var routeId = $"route_{tenant.TenantName}";
                var clusterId = $"cluster_{tenant.TenantName}";

                // Create route with host-based routing
                config.Routes[routeId] = new YarpRoute
                {
                    ClusterId = clusterId,
                    Match = new YarpMatch
                    {
                        Path = new List<string> { $"/{tenant.TenantName}/{{**catch-all}}" },
                        Hosts = new List<string> { "api.demodeck.xyz" }
                    },
                    Transforms = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["RequestHeader"] = tenant.CustomHeaders
                        },
                        new Dictionary<string, object>
                        {
                            ["PathPattern"] = new Dictionary<string, string>
                            {
                                ["Pattern"] = "/{**catch-all}"
                            }
                        }
                    }
                };

                // Get region-specific gateway URL
                var region = regions.FirstOrDefault(r => r.RegionId == tenant.PrimaryRegion);
                var gatewayUrl = region?.ApiGatewayUrl ?? "http://default-gateway";

                // Create cluster pointing to regional gateway
                config.Clusters[clusterId] = new YarpCluster
                {
                    Destinations = new Dictionary<string, YarpDestination>
                    {
                        [$"destination_{tenant.TenantName}"] = new YarpDestination
                        {
                            Address = $"{gatewayUrl}/{tenant.TenantName}/"
                        }
                    },
                    HealthCheck = new YarpHealthCheck
                    {
                        Active = new YarpActiveHealthCheck
                        {
                            Enabled = true,
                            Interval = "00:00:30",
                            Timeout = "00:00:05",
                            Policy = "ConsecutiveFailures",
                            Path = "/health"
                        }
                    }
                };
            }

            return config;
        }
    }
}
