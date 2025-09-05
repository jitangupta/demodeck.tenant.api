namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class InMemoryRegionRepository : IRegionRepository
    {
        private readonly List<Region> _regions;

        public InMemoryRegionRepository()
        {
            _regions = new List<Region>
            {
                new Region
                {
                    RegionId = "us-east",
                    RegionName = "us-east",
                    DisplayName = "US East (Virginia)",
                    IsActive = true,
                    ApiGatewayUrl = "https://api-useast.demodeck.xyz",
                    AvailableVersions = new List<string> { "1.0", "1.5", "2.0", "2.1" },
                    CurrentLoad = 15,
                    RegionMetrics = new Dictionary<string, object>
                    {
                        ["avg_response_time"] = 120,
                        ["active_tenants"] = 15,
                        ["cpu_utilization"] = 65,
                        ["memory_utilization"] = 70
                    }
                },
                new Region
                {
                    RegionId = "us-west",
                    RegionName = "us-west",
                    DisplayName = "US West (Oregon)",
                    IsActive = true,
                    ApiGatewayUrl = "https://api-uswest.demodeck.xyz",
                    AvailableVersions = new List<string> { "1.0", "1.5", "2.0" },
                    CurrentLoad = 8,
                    RegionMetrics = new Dictionary<string, object>
                    {
                        ["avg_response_time"] = 95,
                        ["active_tenants"] = 8,
                        ["cpu_utilization"] = 45,
                        ["memory_utilization"] = 55
                    }
                },
                new Region
                {
                    RegionId = "eu-west",
                    RegionName = "eu-west",
                    DisplayName = "Europe West (Ireland)",
                    IsActive = true,
                    ApiGatewayUrl = "https://api-euwest.demodeck.xyz",
                    AvailableVersions = new List<string> { "1.0", "1.5" },
                    CurrentLoad = 3,
                    RegionMetrics = new Dictionary<string, object>
                    {
                        ["avg_response_time"] = 110,
                        ["active_tenants"] = 3,
                        ["cpu_utilization"] = 25,
                        ["memory_utilization"] = 30
                    }
                }
            };
        }

        public Task<Region?> GetRegionByIdAsync(string regionId)
        {
            var region = _regions.FirstOrDefault(r => r.RegionId == regionId && r.IsActive);
            return Task.FromResult(region);
        }

        public Task<List<Region>> GetAllActiveRegionsAsync()
        {
            return Task.FromResult(_regions.Where(r => r.IsActive).ToList());
        }

        public Task<bool> UpdateRegionMetricsAsync(string regionId, Dictionary<string, object> metrics)
        {
            var region = _regions.FirstOrDefault(r => r.RegionId == regionId);
            if (region == null) return Task.FromResult(false);

            foreach (var metric in metrics)
            {
                region.RegionMetrics[metric.Key] = metric.Value;
            }

            return Task.FromResult(true);
        }
    }
}
