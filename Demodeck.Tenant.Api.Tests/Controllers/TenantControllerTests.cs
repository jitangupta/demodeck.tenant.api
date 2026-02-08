using Demodeck.Tenant.Api.Controllers;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Controllers;

public class TenantControllerTests
{
    private readonly Mock<ITenantService> _mockTenantService;
    private readonly Mock<ILogger<TenantController>> _mockLogger;
    private readonly TenantController _controller;

    public TenantControllerTests()
    {
        _mockTenantService = new Mock<ITenantService>();
        _mockLogger = new Mock<ILogger<TenantController>>();
        _controller = new TenantController(_mockTenantService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllTenants_ReturnsOkResult()
    {
        _mockTenantService.Setup(s => s.GetAllTenantsAsync()).ReturnsAsync(new List<TenantDto>());

        var result = await _controller.GetAllTenants();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllTenants_ReturnsApiResponseWithSuccessTrue()
    {
        _mockTenantService.Setup(s => s.GetAllTenantsAsync()).ReturnsAsync(new List<TenantDto>());

        var result = await _controller.GetAllTenants();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<TenantDto>>;

        Assert.NotNull(response);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetAllTenants_ReturnsCorrectTenantCount()
    {
        var tenants = new List<TenantDto>
        {
            new() { TenantName = "acme" },
            new() { TenantName = "globalx" },
            new() { TenantName = "initech" }
        };
        _mockTenantService.Setup(s => s.GetAllTenantsAsync()).ReturnsAsync(tenants);

        var result = await _controller.GetAllTenants();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<TenantDto>>;

        Assert.Equal(3, response!.Data!.Count);
    }

    [Fact]
    public async Task GetAllTenants_MessageContainsTenantCount()
    {
        var tenants = new List<TenantDto>
        {
            new() { TenantName = "acme" },
            new() { TenantName = "globalx" }
        };
        _mockTenantService.Setup(s => s.GetAllTenantsAsync()).ReturnsAsync(tenants);

        var result = await _controller.GetAllTenants();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<TenantDto>>;

        Assert.Contains("2", response!.Message);
    }

    [Fact]
    public async Task GetAllTenants_ReturnsEmptyListWhenNoTenants()
    {
        _mockTenantService.Setup(s => s.GetAllTenantsAsync()).ReturnsAsync(new List<TenantDto>());

        var result = await _controller.GetAllTenants();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<TenantDto>>;

        Assert.Empty(response!.Data!);
    }

    [Fact]
    public async Task GetTenant_WithExistingTenant_ReturnsOkResult()
    {
        var tenant = new TenantDto { TenantName = "acme", DisplayName = "Acme Corp" };
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("acme")).ReturnsAsync(tenant);

        var result = await _controller.GetTenant("acme");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetTenant_WithExistingTenant_ReturnsApiResponseWithSuccessTrue()
    {
        var tenant = new TenantDto { TenantName = "acme", DisplayName = "Acme Corp" };
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("acme")).ReturnsAsync(tenant);

        var result = await _controller.GetTenant("acme");
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<TenantDto>;

        Assert.True(response!.Success);
    }

    [Fact]
    public async Task GetTenant_WithExistingTenant_ReturnsTenantData()
    {
        var tenant = new TenantDto { TenantName = "acme", DisplayName = "Acme Corp" };
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("acme")).ReturnsAsync(tenant);

        var result = await _controller.GetTenant("acme");
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<TenantDto>;

        Assert.Equal("acme", response!.Data!.TenantName);
        Assert.Equal("Acme Corp", response.Data.DisplayName);
    }

    [Fact]
    public async Task GetTenant_WithNonExistingTenant_ReturnsNotFound()
    {
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("nonexistent")).ReturnsAsync((TenantDto?)null);

        var result = await _controller.GetTenant("nonexistent");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetTenant_WithNonExistingTenant_ReturnsErrorCodeTenantNotFound()
    {
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("nonexistent")).ReturnsAsync((TenantDto?)null);

        var result = await _controller.GetTenant("nonexistent");
        var notFoundResult = result as NotFoundObjectResult;
        var response = notFoundResult!.Value as ApiResponse<object>;

        Assert.Equal("TENANT_NOT_FOUND", response!.ErrorCode);
    }

    [Fact]
    public async Task GetTenant_WithNonExistingTenant_ReturnsSuccessFalse()
    {
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("nonexistent")).ReturnsAsync((TenantDto?)null);

        var result = await _controller.GetTenant("nonexistent");
        var notFoundResult = result as NotFoundObjectResult;
        var response = notFoundResult!.Value as ApiResponse<object>;

        Assert.False(response!.Success);
    }

    [Fact]
    public async Task GetTenant_CallsServiceWithCorrectTenantName()
    {
        _mockTenantService.Setup(s => s.GetTenantByNameAsync("acme")).ReturnsAsync((TenantDto?)null);

        await _controller.GetTenant("acme");

        _mockTenantService.Verify(s => s.GetTenantByNameAsync("acme"), Times.Once);
    }
}
