namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<List<TenantDto>> GetAllTenantsAsync()
        {
            return await _tenantRepository.GetAllTenantsAsync();
        }

        public async Task<TenantDto?> GetTenantByNameAsync(string tenantName)
        {
            return await _tenantRepository.GetTenantByNameAsync(tenantName);
        }
    }
}