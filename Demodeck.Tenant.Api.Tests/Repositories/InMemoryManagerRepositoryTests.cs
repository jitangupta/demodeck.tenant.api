using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;

namespace Demodeck.Tenant.Api.Tests.Repositories;

public class InMemoryManagerRepositoryTests
{
    private readonly InMemoryManagerRepository _repository;

    public InMemoryManagerRepositoryTests()
    {
        _repository = new InMemoryManagerRepository();
    }

    [Fact]
    public async Task GetAllManagersAsync_ReturnsSeedData()
    {
        var managers = await _repository.GetAllManagersAsync();
        Assert.Equal(2, managers.Count);
    }

    [Fact]
    public async Task GetAllManagersAsync_ReturnsOnlyActiveManagers()
    {
        var managers = await _repository.GetAllManagersAsync();
        Assert.All(managers, m => Assert.True(m.IsActive));
    }

    [Fact]
    public async Task GetAllManagersAsync_ContainsAdminAndManager()
    {
        var managers = await _repository.GetAllManagersAsync();
        var usernames = managers.Select(m => m.Username).ToList();
        Assert.Contains("admin", usernames);
        Assert.Contains("manager", usernames);
    }

    [Fact]
    public async Task GetManagerByUsernameAsync_WithExistingUsername_ReturnsManager()
    {
        var manager = await _repository.GetManagerByUsernameAsync("admin");
        Assert.NotNull(manager);
        Assert.Equal("mgr_001", manager.Id);
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("Admin")]
    [InlineData("admin")]
    public async Task GetManagerByUsernameAsync_IsCaseInsensitive(string username)
    {
        var manager = await _repository.GetManagerByUsernameAsync(username);
        Assert.NotNull(manager);
        Assert.Equal("admin", manager.Username);
    }

    [Fact]
    public async Task GetManagerByUsernameAsync_WithNonExistingUsername_ReturnsNull()
    {
        var manager = await _repository.GetManagerByUsernameAsync("nobody");
        Assert.Null(manager);
    }

    [Fact]
    public async Task GetManagerByUsernameAsync_AdminHasSuperAdminRole()
    {
        var manager = await _repository.GetManagerByUsernameAsync("admin");
        Assert.NotNull(manager);
        Assert.Equal("SuperAdmin", manager.Role);
    }

    [Fact]
    public async Task GetManagerByUsernameAsync_ManagerHasAdminRole()
    {
        var manager = await _repository.GetManagerByUsernameAsync("manager");
        Assert.NotNull(manager);
        Assert.Equal("Admin", manager.Role);
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithCorrectAdminPassword_ReturnsTrue()
    {
        var manager = await _repository.GetManagerByUsernameAsync("admin");
        Assert.NotNull(manager);
        var result = await _repository.ValidatePasswordAsync(manager, "password123");
        Assert.True(result);
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithCorrectManagerPassword_ReturnsTrue()
    {
        var manager = await _repository.GetManagerByUsernameAsync("manager");
        Assert.NotNull(manager);
        var result = await _repository.ValidatePasswordAsync(manager, "manager123");
        Assert.True(result);
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithIncorrectPassword_ReturnsFalse()
    {
        var manager = await _repository.GetManagerByUsernameAsync("admin");
        Assert.NotNull(manager);
        var result = await _repository.ValidatePasswordAsync(manager, "wrongpassword");
        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithEmptyPassword_ReturnsFalse()
    {
        var manager = await _repository.GetManagerByUsernameAsync("admin");
        Assert.NotNull(manager);
        var result = await _repository.ValidatePasswordAsync(manager, "");
        Assert.False(result);
    }
}
