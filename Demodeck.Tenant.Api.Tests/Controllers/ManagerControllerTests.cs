using Demodeck.Tenant.Api.Controllers;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Controllers;

public class ManagerControllerTests
{
    private readonly Mock<IManagerService> _mockManagerService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<ManagerController>> _mockLogger;
    private readonly ManagerController _controller;

    private readonly Manager _testManager = new()
    {
        Id = "mgr_001",
        Username = "admin",
        Email = "admin@demodeck.xyz",
        PasswordHash = "hashed_password",
        Role = "SuperAdmin",
        IsActive = true,
        CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    public ManagerControllerTests()
    {
        _mockManagerService = new Mock<IManagerService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<ManagerController>>();
        _controller = new ManagerController(
            _mockManagerService.Object,
            _mockJwtService.Object,
            _mockLogger.Object);
    }

    // --- GetAllManagers ---

    [Fact]
    public async Task GetAllManagers_ReturnsOkResult()
    {
        _mockManagerService.Setup(s => s.GetAllManagersAsync()).ReturnsAsync(new List<Manager>());

        var result = await _controller.GetAllManagers();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllManagers_ReturnsCorrectManagerCount()
    {
        var managers = new List<Manager> { _testManager };
        _mockManagerService.Setup(s => s.GetAllManagersAsync()).ReturnsAsync(managers);

        var result = await _controller.GetAllManagers();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Manager>>;

        Assert.Single(response!.Data!);
    }

    [Fact]
    public async Task GetAllManagers_StripsPasswordHashFromResponse()
    {
        var managers = new List<Manager> { _testManager };
        _mockManagerService.Setup(s => s.GetAllManagersAsync()).ReturnsAsync(managers);

        var result = await _controller.GetAllManagers();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Manager>>;

        Assert.All(response!.Data!, m => Assert.Equal(string.Empty, m.PasswordHash));
    }

    [Fact]
    public async Task GetAllManagers_PreservesOtherManagerFields()
    {
        var managers = new List<Manager> { _testManager };
        _mockManagerService.Setup(s => s.GetAllManagersAsync()).ReturnsAsync(managers);

        var result = await _controller.GetAllManagers();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Manager>>;
        var manager = response!.Data!.First();

        Assert.Equal("mgr_001", manager.Id);
        Assert.Equal("admin", manager.Username);
        Assert.Equal("admin@demodeck.xyz", manager.Email);
        Assert.Equal("SuperAdmin", manager.Role);
        Assert.True(manager.IsActive);
    }

    [Fact]
    public async Task GetAllManagers_ReturnsEmptyListWhenNoManagers()
    {
        _mockManagerService.Setup(s => s.GetAllManagersAsync()).ReturnsAsync(new List<Manager>());

        var result = await _controller.GetAllManagers();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Manager>>;

        Assert.Empty(response!.Data!);
    }

    // --- Authenticate ---

    [Fact]
    public async Task Authenticate_WithEmptyUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest { Username = "", Password = "password" };

        var result = await _controller.Authenticate(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequest.Value as ApiResponse<object>;
        Assert.Equal("INVALID_INPUT", response!.ErrorCode);
    }

    [Fact]
    public async Task Authenticate_WithEmptyPassword_ReturnsBadRequest()
    {
        var request = new LoginRequest { Username = "admin", Password = "" };

        var result = await _controller.Authenticate(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithWhitespaceOnlyCredentials_ReturnsBadRequest()
    {
        var request = new LoginRequest { Username = "   ", Password = "   " };

        var result = await _controller.Authenticate(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithBothEmpty_ReturnsBadRequestWithInvalidInput()
    {
        var request = new LoginRequest { Username = "", Password = "" };

        var result = await _controller.Authenticate(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequest.Value as ApiResponse<object>;
        Assert.False(response!.Success);
        Assert.Equal("INVALID_INPUT", response.ErrorCode);
    }

    [Fact]
    public async Task Authenticate_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Username = "admin", Password = "wrong" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "wrong"))
            .ReturnsAsync((Manager?)null);

        var result = await _controller.Authenticate(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithInvalidCredentials_ReturnsErrorCodeInvalidCredentials()
    {
        var request = new LoginRequest { Username = "admin", Password = "wrong" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "wrong"))
            .ReturnsAsync((Manager?)null);

        var result = await _controller.Authenticate(request);
        var unauthorized = result as UnauthorizedObjectResult;
        var response = unauthorized!.Value as ApiResponse<object>;

        Assert.Equal("INVALID_CREDENTIALS", response!.ErrorCode);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsOkResult()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsTokenInResponse()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponse>;

        Assert.Equal("test-jwt-token", response!.Data!.Token);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsTokenTypeBearer()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponse>;

        Assert.Equal("Bearer", response!.Data!.TokenType);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ClearsPasswordHashInResponse()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponse>;

        Assert.Equal(string.Empty, response!.Data!.Manager.PasswordHash);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_PreservesManagerFields()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponse>;
        var manager = response!.Data!.Manager;

        Assert.Equal("mgr_001", manager.Id);
        Assert.Equal("admin", manager.Username);
        Assert.Equal("admin@demodeck.xyz", manager.Email);
        Assert.Equal("SuperAdmin", manager.Role);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_CallsGenerateTokenOnce()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        await _controller.Authenticate(request);

        _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<Manager>()), Times.Once);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ExpiresAtIsInFuture()
    {
        var request = new LoginRequest { Username = "admin", Password = "password123" };
        _mockManagerService.Setup(s => s.ValidateCredentialsAsync("admin", "password123"))
            .ReturnsAsync(_testManager);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<Manager>()))
            .Returns("test-jwt-token");

        var result = await _controller.Authenticate(request);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponse>;

        Assert.True(response!.Data!.ExpiresAt > DateTime.Now);
    }
}
