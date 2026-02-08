using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.Extensions.Options;

namespace Demodeck.Tenant.Api.Tests.Repositories;

public class InMemoryTenantRepositoryTests
{
    private readonly InMemoryTenantRepository _repository;
    private readonly ApiEndpointsSettings _settings;

    public InMemoryTenantRepositoryTests()
    {
        _settings = new ApiEndpointsSettings
        {
            AuthAPI = "http://test-auth-api.example.com",
            ProductAPI = "http://test-product-api.example.com"
        };
        _repository = new InMemoryTenantRepository(Options.Create(_settings));
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsSeedData()
    {
        var tenants = await _repository.GetAllTenantsAsync();
        Assert.Equal(4, tenants.Count);
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsOnlyActiveTenants()
    {
        var tenants = await _repository.GetAllTenantsAsync();
        Assert.All(tenants, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetAllTenantsAsync_ContainsExpectedTenantNames()
    {
        var tenants = await _repository.GetAllTenantsAsync();
        var names = tenants.Select(t => t.TenantName).ToList();
        Assert.Contains("acme", names);
        Assert.Contains("globalx", names);
        Assert.Contains("initech", names);
        Assert.Contains("umbrella", names);
    }

    [Fact]
    public async Task GetTenantByNameAsync_WithExistingName_ReturnsTenant()
    {
        var tenant = await _repository.GetTenantByNameAsync("acme");
        Assert.NotNull(tenant);
        Assert.Equal("tnt_acme001", tenant.TenantId);
    }

    [Theory]
    [InlineData("ACME")]
    [InlineData("Acme")]
    [InlineData("acme")]
    public async Task GetTenantByNameAsync_IsCaseInsensitive(string tenantName)
    {
        var tenant = await _repository.GetTenantByNameAsync(tenantName);
        Assert.NotNull(tenant);
        Assert.Equal("acme", tenant.TenantName);
    }

    [Fact]
    public async Task GetTenantByNameAsync_SetsAuthApiFromConfig()
    {
        var tenant = await _repository.GetTenantByNameAsync("acme");
        Assert.NotNull(tenant);
        Assert.Equal(_settings.AuthAPI, tenant.AuthAPI);
    }

    [Fact]
    public async Task GetTenantByNameAsync_SetsProductApiFromConfig()
    {
        var tenant = await _repository.GetTenantByNameAsync("acme");
        Assert.NotNull(tenant);
        Assert.Equal(_settings.ProductAPI, tenant.ProductAPI);
    }

    [Fact]
    public async Task GetTenantByNameAsync_WithNonExistingName_ThrowsNullReferenceException()
    {
        // Documents existing bug: GetTenantByNameAsync assigns properties on null
        // when tenant is not found, causing NullReferenceException
        await Assert.ThrowsAsync<NullReferenceException>(
            () => _repository.GetTenantByNameAsync("nonexistent"));
    }
}
