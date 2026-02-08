using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Services;

public class TenantServiceTests
{
    private readonly Mock<ITenantRepository> _mockRepository;
    private readonly TenantService _service;

    public TenantServiceTests()
    {
        _mockRepository = new Mock<ITenantRepository>();
        _service = new TenantService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllTenantsAsync_DelegatesToRepository()
    {
        _mockRepository.Setup(r => r.GetAllTenantsAsync()).ReturnsAsync(new List<TenantDto>());
        await _service.GetAllTenantsAsync();
        _mockRepository.Verify(r => r.GetAllTenantsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsRepositoryResult()
    {
        var expected = new List<TenantDto>
        {
            new() { TenantName = "acme", DisplayName = "Acme Corp" },
            new() { TenantName = "globalx", DisplayName = "GlobalX" }
        };
        _mockRepository.Setup(r => r.GetAllTenantsAsync()).ReturnsAsync(expected);

        var result = await _service.GetAllTenantsAsync();

        Assert.Equal(2, result.Count);
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsEmptyListFromRepository()
    {
        _mockRepository.Setup(r => r.GetAllTenantsAsync()).ReturnsAsync(new List<TenantDto>());

        var result = await _service.GetAllTenantsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTenantByNameAsync_DelegatesToRepository()
    {
        _mockRepository.Setup(r => r.GetTenantByNameAsync("acme")).ReturnsAsync((TenantDto?)null);
        await _service.GetTenantByNameAsync("acme");
        _mockRepository.Verify(r => r.GetTenantByNameAsync("acme"), Times.Once);
    }

    [Fact]
    public async Task GetTenantByNameAsync_ReturnsTenantWhenFound()
    {
        var expected = new TenantDto { TenantName = "acme", DisplayName = "Acme Corp" };
        _mockRepository.Setup(r => r.GetTenantByNameAsync("acme")).ReturnsAsync(expected);

        var result = await _service.GetTenantByNameAsync("acme");

        Assert.NotNull(result);
        Assert.Equal("acme", result.TenantName);
    }

    [Fact]
    public async Task GetTenantByNameAsync_ReturnsNullWhenNotFound()
    {
        _mockRepository.Setup(r => r.GetTenantByNameAsync("nonexistent")).ReturnsAsync((TenantDto?)null);

        var result = await _service.GetTenantByNameAsync("nonexistent");

        Assert.Null(result);
    }
}
