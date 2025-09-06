using Microsoft.AspNetCore.Mvc;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;

namespace Demodeck.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(IManagerService managerService, IJwtService jwtService, ILogger<ManagerController> logger)
        {
            _managerService = managerService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Manager>>), 200)]
        public async Task<IActionResult> GetAllManagers()
        {
            var managers = await _managerService.GetAllManagersAsync();

            var managersWithoutPasswords = managers.Select(m => new Manager
            {
                Id = m.Id,
                Username = m.Username,
                Email = m.Email,
                Role = m.Role,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                PasswordHash = string.Empty
            }).ToList();

            return Ok(new ApiResponse<List<Manager>>
            {
                Success = true,
                Data = managersWithoutPasswords,
                Message = $"Retrieved {managersWithoutPasswords.Count} managers"
            });
        }

        [HttpPost("token")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Username and password are required",
                    ErrorCode = "INVALID_INPUT"
                });
            }

            var manager = await _managerService.ValidateCredentialsAsync(request.Username, request.Password);

            if (manager == null)
            {
                _logger.LogWarning("Authentication failed for username: {Username}", request.Username);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid username or password",
                    ErrorCode = "INVALID_CREDENTIALS"
                });
            }

            var token = _jwtService.GenerateToken(manager);

            _logger.LogInformation("Manager {Username} authenticated successfully", manager.Username);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Data = new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.Now.AddMinutes(60),
                    TokenType = "Bearer",
                    Manager = new Manager
                    {
                        Id = manager.Id,
                        Username = manager.Username,
                        Email = manager.Email,
                        Role = manager.Role,
                        IsActive = manager.IsActive,
                        CreatedAt = manager.CreatedAt,
                        PasswordHash = string.Empty
                    }
                },
                Message = "Authentication successful"
            });
        }
    }
}