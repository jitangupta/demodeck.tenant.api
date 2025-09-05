using Demodeck.Tenant.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demodeck.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ITenantHealthService _healthService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ITenantHealthService healthService, ILogger<HealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetApiHealth()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0",
                service = "Demodeck.Tenant.Api"
            });
        }

        [HttpGet("tenant/{tenantId}/region/{region}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        public async Task<IActionResult> GetTenantHealth(string tenantId, string region)
        {
            var health = await _healthService.GetTenantHealthAsync(tenantId, region);
            return Ok(health);
        }

        [HttpGet("region/{region}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        public async Task<IActionResult> GetRegionHealth(string region)
        {
            var health = await _healthService.GetRegionHealthAsync(region);
            return Ok(health);
        }

        [HttpPost("tenant/{tenantId}/region/{region}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateTenantHealth(string tenantId, string region, [FromBody] Dictionary<string, object> healthData)
        {
            await _healthService.UpdateTenantHealthAsync(tenantId, region, healthData);

            _logger.LogInformation("Updated health for tenant {TenantId} in region {Region}", tenantId, region);

            return Ok(new { message = "Health data updated successfully" });
        }
    }
}
