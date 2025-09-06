using Microsoft.AspNetCore.Mvc;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;

namespace Demodeck.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantService tenantService, ILogger<TenantController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TenantDto>>), 200)]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();

            return Ok(new ApiResponse<List<TenantDto>>
            {
                Success = true,
                Data = tenants,
                Message = $"Retrieved {tenants.Count} tenants"
            });
        }

        [HttpGet("{tenantName}")]
        [ProducesResponseType(typeof(ApiResponse<TenantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetTenant(string tenantName)
        {
            var tenant = await _tenantService.GetTenantByNameAsync(tenantName);

            if (tenant == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tenant not found",
                    ErrorCode = "TENANT_NOT_FOUND"
                });
            }

            return Ok(new ApiResponse<TenantDto>
            {
                Success = true,
                Data = tenant,
                Message = "Tenant retrieved successfully"
            });
        }
    }
}
