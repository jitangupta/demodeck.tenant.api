namespace Demodeck.Tenant.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Demodeck.Tenant.Api.Models;
    using Demodeck.Tenant.Api.Services;

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

        [HttpGet("{tenantName}/routing")]
        [ProducesResponseType(typeof(ApiResponse<TenantRoutingInfo>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetTenantRouting(string tenantName)
        {
            var routingInfo = await _tenantService.GetTenantRoutingInfoAsync(tenantName);

            if (routingInfo == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tenant not found",
                    ErrorCode = "TENANT_NOT_FOUND"
                });
            }

            return Ok(new ApiResponse<TenantRoutingInfo>
            {
                Success = true,
                Data = routingInfo,
                Message = "Tenant routing information retrieved"
            });
        }

        [HttpGet("routing")]
        [ProducesResponseType(typeof(ApiResponse<List<TenantRoutingInfo>>), 200)]
        public async Task<IActionResult> GetAllTenantRouting([FromQuery] string? region = null)
        {
            var routingInfos = await _tenantService.GetAllTenantRoutingInfoAsync(region);

            return Ok(new ApiResponse<List<TenantRoutingInfo>>
            {
                Success = true,
                Data = routingInfos,
                Message = $"Retrieved {routingInfos.Count} tenant routing configurations"
            });
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<GlobalTenant>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenantName) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "TenantName and DisplayName are required",
                    ErrorCode = "INVALID_INPUT"
                });
            }

            var tenant = await _tenantService.CreateTenantAsync(request);

            if (tenant == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tenant name already exists",
                    ErrorCode = "TENANT_EXISTS"
                });
            }

            _logger.LogInformation("Created new tenant {TenantName} with ID {TenantId}",
                tenant.TenantName, tenant.TenantId);

            return CreatedAtAction(nameof(GetTenantRouting),
                new { tenantName = tenant.TenantName },
                new ApiResponse<GlobalTenant>
                {
                    Success = true,
                    Data = tenant,
                    Message = "Tenant created successfully"
                });
        }

        [HttpPut("{tenantId}/version")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateTenantVersion(string tenantId, [FromBody] UpdateTenantVersionRequest request)
        {
            if (request.TenantId != tenantId)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "TenantId in URL must match request body",
                    ErrorCode = "TENANT_ID_MISMATCH"
                });
            }

            var success = await _tenantService.UpdateTenantVersionAsync(request);

            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update tenant version",
                    ErrorCode = "UPDATE_FAILED"
                });
            }

            _logger.LogInformation("Updated tenant {TenantId} in region {Region} to version {Version} by {UpdatedBy}",
                tenantId, request.Region, request.TargetVersion, request.UpdatedBy);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Tenant version updated to {request.TargetVersion} in region {request.Region}",
                Data = new
                {
                    tenantId,
                    region = request.Region,
                    newVersion = request.TargetVersion,
                    trafficPercentage = request.TrafficPercentage
                }
            });
        }
    }
}
