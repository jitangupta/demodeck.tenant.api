using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Services;

public class ManagerServiceTests
{
    private readonly Mock<IManagerRepository> _mockRepository;
    private readonly ManagerService _service;

    public ManagerServiceTests()
    {
        _mockRepository = new Mock<IManagerRepository>();
        _service = new ManagerService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllManagersAsync_DelegatesToRepository()
    {
        _mockRepository.Setup(r => r.GetAllManagersAsync()).ReturnsAsync(new List<Manager>());
        await _service.GetAllManagersAsync();
        _mockRepository.Verify(r => r.GetAllManagersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllManagersAsync_ReturnsRepositoryResult()
    {
        var expected = new List<Manager>
        {
            new() { Id = "mgr_001", Username = "admin" },
            new() { Id = "mgr_002", Username = "manager" }
        };
        _mockRepository.Setup(r => r.GetAllManagersAsync()).ReturnsAsync(expected);

        var result = await _service.GetAllManagersAsync();

        Assert.Equal(2, result.Count);
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithNonExistingUser_ReturnsNull()
    {
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("nobody"))
            .ReturnsAsync((Manager?)null);

        var result = await _service.ValidateCredentialsAsync("nobody", "password");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithNonExistingUser_DoesNotCallValidatePassword()
    {
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("nobody"))
            .ReturnsAsync((Manager?)null);

        await _service.ValidateCredentialsAsync("nobody", "password");

        _mockRepository.Verify(
            r => r.ValidatePasswordAsync(It.IsAny<Manager>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithExistingUserInvalidPassword_ReturnsNull()
    {
        var manager = new Manager { Id = "mgr_001", Username = "admin" };
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("admin")).ReturnsAsync(manager);
        _mockRepository.Setup(r => r.ValidatePasswordAsync(manager, "wrong")).ReturnsAsync(false);

        var result = await _service.ValidateCredentialsAsync("admin", "wrong");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithValidCredentials_ReturnsManager()
    {
        var manager = new Manager { Id = "mgr_001", Username = "admin" };
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("admin")).ReturnsAsync(manager);
        _mockRepository.Setup(r => r.ValidatePasswordAsync(manager, "password123")).ReturnsAsync(true);

        var result = await _service.ValidateCredentialsAsync("admin", "password123");

        Assert.NotNull(result);
        Assert.Equal("mgr_001", result.Id);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CallsGetManagerByUsernameWithCorrectUsername()
    {
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("admin"))
            .ReturnsAsync((Manager?)null);

        await _service.ValidateCredentialsAsync("admin", "password");

        _mockRepository.Verify(r => r.GetManagerByUsernameAsync("admin"), Times.Once);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CallsValidatePasswordWithCorrectArguments()
    {
        var manager = new Manager { Id = "mgr_001", Username = "admin" };
        _mockRepository.Setup(r => r.GetManagerByUsernameAsync("admin")).ReturnsAsync(manager);
        _mockRepository.Setup(r => r.ValidatePasswordAsync(manager, "pass123")).ReturnsAsync(true);

        await _service.ValidateCredentialsAsync("admin", "pass123");

        _mockRepository.Verify(r => r.ValidatePasswordAsync(manager, "pass123"), Times.Once);
    }
}
