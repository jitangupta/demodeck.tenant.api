namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly List<TenantDto> _tenants = new();

        public InMemoryTenantRepository()
        {
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
                    CurrentVersion = "1.1",
                    ServiceName = "acme-v1-1-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_acme001",
                        ["X-Tenant-Version"] = "1.1",
                        ["X-Tenant-Tier"] = "Enterprise"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_globalx001", 
                    TenantName = "globalx",
                    DisplayName = "GlobalX Industries",
                    PrimaryRegion = "westus",
                    CurrentVersion = "1.0",
                    ServiceName = "globalx-v1-0-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_globalx001",
                        ["X-Tenant-Version"] = "1.0",
                        ["X-Tenant-Tier"] = "Standard"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_initech001",
                    TenantName = "initech",
                    DisplayName = "Initech LLC",
                    PrimaryRegion = "centralus",
                    CurrentVersion = "1.2",
                    ServiceName = "initech-v1-2-service",
                    IsHealthy = false,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_initech001",
                        ["X-Tenant-Version"] = "1.2",
                        ["X-Tenant-Tier"] = "Premium"
                    }
                },
                new TenantDto
                {
                    TenantId = "tnt_umbrella001",
                    TenantName = "umbrella",
                    DisplayName = "Umbrella Corp",
                    PrimaryRegion = "westeurope",
                    CurrentVersion = "2.0",
                    ServiceName = "umbrella-v2-0-service",
                    IsHealthy = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    CustomHeaders = new Dictionary<string, string>
                    {
                        ["X-Tenant-Id"] = "tnt_umbrella001",
                        ["X-Tenant-Version"] = "2.0",
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
            return Task.FromResult(tenant);
        }
    }
}