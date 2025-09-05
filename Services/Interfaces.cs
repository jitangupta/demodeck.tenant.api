namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public interface ITenantRepository
    {
        Task<GlobalTenant?> GetTenantByIdAsync(string tenantId);
        Task<GlobalTenant?> GetTenantByNameAsync(string tenantName);
        Task<List<GlobalTenant>> GetAllTenantsAsync();
        Task<List<GlobalTenant>> GetTenantsByRegionAsync(string region);
        Task<bool> CreateTenantAsync(GlobalTenant tenant);
        Task<bool> UpdateTenantAsync(GlobalTenant tenant);
        Task<bool> DeleteTenantAsync(string tenantId);
    }

    public interface IRegionRepository
    {
        Task<Region?> GetRegionByIdAsync(string regionId);
        Task<List<Region>> GetAllActiveRegionsAsync();
        Task<bool> UpdateRegionMetricsAsync(string regionId, Dictionary<string, object> metrics);
    }

    public interface ITenantService
    {
        Task<TenantRoutingInfo?> GetTenantRoutingInfoAsync(string tenantName);
        Task<List<TenantRoutingInfo>> GetAllTenantRoutingInfoAsync(string? region = null);
        Task<bool> UpdateTenantVersionAsync(UpdateTenantVersionRequest request);
        Task<GlobalTenant?> CreateTenantAsync(CreateTenantRequest request);
    }

    public interface IYarpConfigService
    {
        Task<YarpConfiguration> GenerateYarpConfigAsync(string region);
        Task<YarpConfiguration> GenerateGlobalYarpConfigAsync();
    }

    public interface ITenantHealthService
    {
        Task<Dictionary<string, object>> GetTenantHealthAsync(string tenantId, string region);
        Task<Dictionary<string, object>> GetRegionHealthAsync(string region);
        Task UpdateTenantHealthAsync(string tenantId, string region, Dictionary<string, object> healthData);
    }
}
