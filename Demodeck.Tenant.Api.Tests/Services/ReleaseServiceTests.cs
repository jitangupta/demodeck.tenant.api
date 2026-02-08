using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Services;

public class ReleaseServiceTests
{
    private readonly Mock<IReleaseRepository> _mockRepository;
    private readonly ReleaseService _service;

    public ReleaseServiceTests()
    {
        _mockRepository = new Mock<IReleaseRepository>();
        _service = new ReleaseService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllReleasesAsync_DelegatesToRepository()
    {
        _mockRepository.Setup(r => r.GetAllReleasesAsync()).ReturnsAsync(new List<Release>());
        await _service.GetAllReleasesAsync();
        _mockRepository.Verify(r => r.GetAllReleasesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllReleasesAsync_ReturnsRepositoryResult()
    {
        var expected = new List<Release>
        {
            new() { Id = "rel_001", ReleaseName = "v1" },
            new() { Id = "rel_002", ReleaseName = "v2" }
        };
        _mockRepository.Setup(r => r.GetAllReleasesAsync()).ReturnsAsync(expected);

        var result = await _service.GetAllReleasesAsync();

        Assert.Equal(2, result.Count);
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateReleaseAsync_DelegatesToRepository()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        _mockRepository.Setup(r => r.CreateReleaseAsync(release)).ReturnsAsync(release);

        await _service.CreateReleaseAsync(release);

        _mockRepository.Verify(r => r.CreateReleaseAsync(release), Times.Once);
    }

    [Fact]
    public async Task CreateReleaseAsync_ReturnsCreatedRelease()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        var created = new Release { Id = "rel_abc", ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        _mockRepository.Setup(r => r.CreateReleaseAsync(release)).ReturnsAsync(created);

        var result = await _service.CreateReleaseAsync(release);

        Assert.Equal("rel_abc", result.Id);
    }

    [Fact]
    public async Task UpdateReleaseAsync_DelegatesToRepository()
    {
        var release = new Release { Id = "rel_001", ReleaseName = "Updated" };
        _mockRepository.Setup(r => r.UpdateReleaseAsync(release)).ReturnsAsync(release);

        await _service.UpdateReleaseAsync(release);

        _mockRepository.Verify(r => r.UpdateReleaseAsync(release), Times.Once);
    }

    [Fact]
    public async Task UpdateReleaseAsync_ReturnsUpdatedRelease()
    {
        var release = new Release { Id = "rel_001", ReleaseName = "Updated" };
        _mockRepository.Setup(r => r.UpdateReleaseAsync(release)).ReturnsAsync(release);

        var result = await _service.UpdateReleaseAsync(release);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.ReleaseName);
    }

    [Fact]
    public async Task UpdateReleaseAsync_ReturnsNullWhenNotFound()
    {
        var release = new Release { Id = "rel_nonexistent" };
        _mockRepository.Setup(r => r.UpdateReleaseAsync(release)).ReturnsAsync((Release?)null);

        var result = await _service.UpdateReleaseAsync(release);

        Assert.Null(result);
    }
}
