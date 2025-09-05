using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demodeck.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YarpController : ControllerBase
    {
        private readonly IYarpConfigService _yarpConfigService;
        private readonly ILogger<YarpController> _logger;

        public YarpController(IYarpConfigService yarpConfigService, ILogger<YarpController> logger)
        {
            _yarpConfigService = yarpConfigService;
            _logger = logger;
        }

        [HttpGet("config/{region}")]
        [ProducesResponseType(typeof(YarpConfiguration), 200)]
        public async Task<IActionResult> GetRegionalYarpConfig(string region)
        {
            var config = await _yarpConfigService.GenerateYarpConfigAsync(region);

            _logger.LogInformation("Generated YARP config for region {Region} with {RouteCount} routes",
                region, config.Routes.Count);

            return Ok(config);
        }

        [HttpGet("config/global")]
        [ProducesResponseType(typeof(YarpConfiguration), 200)]
        public async Task<IActionResult> GetGlobalYarpConfig()
        {
            var config = await _yarpConfigService.GenerateGlobalYarpConfigAsync();

            _logger.LogInformation("Generated global YARP config with {RouteCount} routes",
                config.Routes.Count);

            return Ok(config);
        }
    }
}
