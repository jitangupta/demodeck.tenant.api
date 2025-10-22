namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;
    using Microsoft.Extensions.Options;

    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly List<TenantDto> _tenants = new();
        private readonly ApiEndpointsSettings _apiEndpoints;

        public InMemoryTenantRepository(IOptions<ApiEndpointsSettings> apiEndpoints)
        {
            _apiEndpoints = apiEndpoints.Value;
            SeedData();
        }

        private void SeedData()
        {
            _tenants.AddRange(new[]
            {
                new TenantDto
                {
                    TenantId = "tnt_acme001",
                    TenantName = "acme",
                    DisplayName = "Acme Corporation",
                    PrimaryRegion = "eastus",
                    CurrentVersion = "2.1.0",
                    ServiceName = "acme-v1-1-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    LogoUrl = "/logos/acme-logo.png",
                    ThemeColor = "#dc2626",
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_acme001",
                        ["X-Tenant-Version"] = "2.1.0",
                        ["X-Tenant-Tier"] = "Enterprise"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_globalx001", 
                    TenantName = "globalx",
                    DisplayName = "GlobalX Industries",
                    PrimaryRegion = "westus",
                    CurrentVersion = "2.1.0",
                    ServiceName = "globalx-v1-0-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    LogoUrl = "/logos/globalx-logo.png",
                    ThemeColor = "#0b1eb1",
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_globalx001",
                        ["X-Tenant-Version"] = "2.1.0",
                        ["X-Tenant-Tier"] = "Standard"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_initech001",
                    TenantName = "initech",
                    DisplayName = "Initech LLC",
                    PrimaryRegion = "centralus",
                    CurrentVersion = "2.0.0",
                    ServiceName = "initech-v1-2-service",
                    IsHealthy = false,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    LogoUrl = "/logos/initech-logo.png",
                    ThemeColor = "#0d7a20",
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_initech001",
                        ["X-Tenant-Version"] = "2.0.0",
                        ["X-Tenant-Tier"] = "Premium"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_umbrella001",
                    TenantName = "umbrella",
                    DisplayName = "Umbrella Corp",
                    PrimaryRegion = "westeurope",
                    CurrentVersion = "2.0.0",
                    ServiceName = "umbrella-v2-0-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    LogoUrl = "/logos/umbrella-logo.png",
                    ThemeColor = "#d3850b",
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_umbrella001",
                        ["X-Tenant-Version"] = "2.0.0",
                        ["X-Tenant-Tier"] = "Enterprise"
                    }
                }
            });
        }

        public Task<List<TenantDto>> GetAllTenantsAsync()
        {
            return Task.FromResult(_tenants.Where(t => t.IsActive).ToList());
        }

        public Task<TenantDto?> GetTenantByNameAsync(string tenantName)
        {
            var tenant = _tenants.FirstOrDefault(t => 
                t.TenantName.Equals(tenantName, StringComparison.OrdinalIgnoreCase) && t.IsActive);

            tenant.AuthAPI = _apiEndpoints.AuthAPI;
            tenant.ProductAPI = _apiEndpoints.ProductAPI;
            return Task.FromResult(tenant);
        }
    }
}