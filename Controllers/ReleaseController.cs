using Microsoft.AspNetCore.Mvc;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;

namespace Demodeck.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly ILogger<ReleaseController> _logger;

        public ReleaseController(IReleaseService releaseService, ILogger<ReleaseController> logger)
        {
            _releaseService = releaseService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Release>>), 200)]
        public async Task<IActionResult> GetAllReleases()
        {
            var releases = await _releaseService.GetAllReleasesAsync();

            return Ok(new ApiResponse<List<Release>>
            {
                Success = true,
                Data = releases,
                Message = $"Retrieved {releases.Count} releases"
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Release>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateRelease([FromBody] Release release)
        {
            if (string.IsNullOrWhiteSpace(release.ReleaseName) || string.IsNullOrWhiteSpace(release.ReleaseVersion))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ReleaseName and ReleaseVersion are required",
                    ErrorCode = "INVALID_INPUT"
                });
            }

            var createdRelease = await _releaseService.CreateReleaseAsync(release);

            _logger.LogInformation("Created new release {ReleaseName} version {ReleaseVersion}",
                createdRelease.ReleaseName, createdRelease.ReleaseVersion);

            return CreatedAtAction(nameof(GetAllReleases), null,
                new ApiResponse<Release>
                {
                    Success = true,
                    Data = createdRelease,
                    Message = "Release created successfully"
                });
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<Release>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateRelease([FromBody] Release release)
        {
            if (string.IsNullOrWhiteSpace(release.Id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Release Id is required",
                    ErrorCode = "INVALID_INPUT"
                });
            }

            var updatedRelease = await _releaseService.UpdateReleaseAsync(release);

            if (updatedRelease == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Release not found",
                    ErrorCode = "RELEASE_NOT_FOUND"
                });
            }

            _logger.LogInformation("Updated release {ReleaseId} - {ReleaseName}",
                updatedRelease.Id, updatedRelease.ReleaseName);

            return Ok(new ApiResponse<Release>
            {
                Success = true,
                Data = updatedRelease,
                Message = "Release updated successfully"
            });
        }
    }
}