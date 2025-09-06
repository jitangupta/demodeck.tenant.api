namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    // Simplified Tenant Repository Interface
    public interface ITenantRepository
    {
        Task<List<TenantDto>> GetAllTenantsAsync();
        Task<TenantDto?> GetTenantByNameAsync(string tenantName);
    }

    // Release Repository Interface
    public interface IReleaseRepository
    {
        Task<List<Release>> GetAllReleasesAsync();
        Task<Release> CreateReleaseAsync(Release release);
        Task<Release?> UpdateReleaseAsync(Release release);
    }

    // Manager Repository Interface
    public interface IManagerRepository
    {
        Task<List<Manager>> GetAllManagersAsync();
        Task<Manager?> GetManagerByUsernameAsync(string username);
        Task<bool> ValidatePasswordAsync(Manager manager, string password);
    }

    // Simplified Tenant Service Interface
    public interface ITenantService
    {
        Task<List<TenantDto>> GetAllTenantsAsync();
        Task<TenantDto?> GetTenantByNameAsync(string tenantName);
    }

    // Release Service Interface
    public interface IReleaseService
    {
        Task<List<Release>> GetAllReleasesAsync();
        Task<Release> CreateReleaseAsync(Release release);
        Task<Release?> UpdateReleaseAsync(Release release);
    }

    // Manager Service Interface
    public interface IManagerService
    {
        Task<List<Manager>> GetAllManagersAsync();
        Task<Manager?> ValidateCredentialsAsync(string username, string password);
    }

    // JWT Service Interface
    public interface IJwtService
    {
        string GenerateToken(Manager manager);
    }
}