using Demodeck.Tenant.Api.Controllers;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demodeck.Tenant.Api.Tests.Controllers;

public class ReleaseControllerTests
{
    private readonly Mock<IReleaseService> _mockReleaseService;
    private readonly Mock<ILogger<ReleaseController>> _mockLogger;
    private readonly ReleaseController _controller;

    public ReleaseControllerTests()
    {
        _mockReleaseService = new Mock<IReleaseService>();
        _mockLogger = new Mock<ILogger<ReleaseController>>();
        _controller = new ReleaseController(_mockReleaseService.Object, _mockLogger.Object);
    }

    // --- GetAllReleases ---

    [Fact]
    public async Task GetAllReleases_ReturnsOkResult()
    {
        _mockReleaseService.Setup(s => s.GetAllReleasesAsync()).ReturnsAsync(new List<Release>());

        var result = await _controller.GetAllReleases();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllReleases_ReturnsApiResponseWithSuccessTrue()
    {
        _mockReleaseService.Setup(s => s.GetAllReleasesAsync()).ReturnsAsync(new List<Release>());

        var result = await _controller.GetAllReleases();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Release>>;

        Assert.True(response!.Success);
    }

    [Fact]
    public async Task GetAllReleases_ReturnsCorrectReleaseCount()
    {
        var releases = new List<Release>
        {
            new() { Id = "rel_001", ReleaseName = "v1.0" },
            new() { Id = "rel_002", ReleaseName = "v2.0" }
        };
        _mockReleaseService.Setup(s => s.GetAllReleasesAsync()).ReturnsAsync(releases);

        var result = await _controller.GetAllReleases();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Release>>;

        Assert.Equal(2, response!.Data!.Count);
    }

    [Fact]
    public async Task GetAllReleases_ReturnsEmptyListWhenNoReleases()
    {
        _mockReleaseService.Setup(s => s.GetAllReleasesAsync()).ReturnsAsync(new List<Release>());

        var result = await _controller.GetAllReleases();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<Release>>;

        Assert.Empty(response!.Data!);
    }

    // --- CreateRelease ---

    [Fact]
    public async Task CreateRelease_WithValidRelease_ReturnsCreatedResult()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        var created = new Release { Id = "rel_abc", ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        _mockReleaseService.Setup(s => s.CreateReleaseAsync(release)).ReturnsAsync(created);

        var result = await _controller.CreateRelease(release);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task CreateRelease_WithValidRelease_ReturnsCreatedReleaseData()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        var created = new Release { Id = "rel_abc", ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        _mockReleaseService.Setup(s => s.CreateReleaseAsync(release)).ReturnsAsync(created);

        var result = await _controller.CreateRelease(release);
        var createdResult = result as CreatedAtActionResult;
        var response = createdResult!.Value as ApiResponse<Release>;

        Assert.Equal("rel_abc", response!.Data!.Id);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateRelease_WithEmptyReleaseName_ReturnsBadRequest()
    {
        var release = new Release { ReleaseName = "", ReleaseVersion = "1.0.0" };

        var result = await _controller.CreateRelease(release);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequest.Value as ApiResponse<object>;
        Assert.Equal("INVALID_INPUT", response!.ErrorCode);
    }

    [Fact]
    public async Task CreateRelease_WithEmptyReleaseVersion_ReturnsBadRequest()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "" };

        var result = await _controller.CreateRelease(release);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRelease_WithBothEmpty_ReturnsBadRequest()
    {
        var release = new Release { ReleaseName = "", ReleaseVersion = "" };

        var result = await _controller.CreateRelease(release);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequest.Value as ApiResponse<object>;
        Assert.Equal("INVALID_INPUT", response!.ErrorCode);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task CreateRelease_WithWhitespaceReleaseName_ReturnsBadRequest()
    {
        var release = new Release { ReleaseName = "   ", ReleaseVersion = "1.0.0" };

        var result = await _controller.CreateRelease(release);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRelease_CallsServiceOnce()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "1.0.0" };
        _mockReleaseService.Setup(s => s.CreateReleaseAsync(release)).ReturnsAsync(release);

        await _controller.CreateRelease(release);

        _mockReleaseService.Verify(s => s.CreateReleaseAsync(release), Times.Once);
    }

    // --- UpdateRelease ---

    [Fact]
    public async Task UpdateRelease_WithValidRelease_ReturnsOkResult()
    {
        var release = new Release { Id = "rel_001", ReleaseName = "Updated", ReleaseVersion = "1.0.1" };
        _mockReleaseService.Setup(s => s.UpdateReleaseAsync(release)).ReturnsAsync(release);

        var result = await _controller.UpdateRelease(release);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRelease_WithValidRelease_ReturnsUpdatedData()
    {
        var release = new Release { Id = "rel_001", ReleaseName = "Updated", ReleaseVersion = "1.0.1" };
        _mockReleaseService.Setup(s => s.UpdateReleaseAsync(release)).ReturnsAsync(release);

        var result = await _controller.UpdateRelease(release);
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<Release>;

        Assert.Equal("Updated", response!.Data!.ReleaseName);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateRelease_WithEmptyId_ReturnsBadRequest()
    {
        var release = new Release { Id = "", ReleaseName = "Test" };

        var result = await _controller.UpdateRelease(release);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequest.Value as ApiResponse<object>;
        Assert.Equal("INVALID_INPUT", response!.ErrorCode);
    }

    [Fact]
    public async Task UpdateRelease_WithWhitespaceId_ReturnsBadRequest()
    {
        var release = new Release { Id = "   ", ReleaseName = "Test" };

        var result = await _controller.UpdateRelease(release);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRelease_WithNonExistingId_ReturnsNotFound()
    {
        var release = new Release { Id = "rel_nonexistent", ReleaseName = "Test" };
        _mockReleaseService.Setup(s => s.UpdateReleaseAsync(release)).ReturnsAsync((Release?)null);

        var result = await _controller.UpdateRelease(release);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRelease_WithNonExistingId_ReturnsErrorCodeReleaseNotFound()
    {
        var release = new Release { Id = "rel_nonexistent", ReleaseName = "Test" };
        _mockReleaseService.Setup(s => s.UpdateReleaseAsync(release)).ReturnsAsync((Release?)null);

        var result = await _controller.UpdateRelease(release);
        var notFound = result as NotFoundObjectResult;
        var response = notFound!.Value as ApiResponse<object>;

        Assert.Equal("RELEASE_NOT_FOUND", response!.ErrorCode);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateRelease_CallsServiceOnce()
    {
        var release = new Release { Id = "rel_001", ReleaseName = "Test" };
        _mockReleaseService.Setup(s => s.UpdateReleaseAsync(release)).ReturnsAsync(release);

        await _controller.UpdateRelease(release);

        _mockReleaseService.Verify(s => s.UpdateReleaseAsync(release), Times.Once);
    }
}
