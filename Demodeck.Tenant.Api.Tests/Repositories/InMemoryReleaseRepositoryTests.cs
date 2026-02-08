using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;

namespace Demodeck.Tenant.Api.Tests.Repositories;

public class InMemoryReleaseRepositoryTests
{
    private readonly InMemoryReleaseRepository _repository;

    public InMemoryReleaseRepositoryTests()
    {
        _repository = new InMemoryReleaseRepository();
    }

    [Fact]
    public async Task GetAllReleasesAsync_ReturnsSeedData()
    {
        var releases = await _repository.GetAllReleasesAsync();
        Assert.Equal(5, releases.Count);
    }

    [Fact]
    public async Task GetAllReleasesAsync_ReturnsOrderedByCreatedDateDescending()
    {
        var releases = await _repository.GetAllReleasesAsync();
        for (int i = 0; i < releases.Count - 1; i++)
        {
            Assert.True(releases[i].CreatedDate >= releases[i + 1].CreatedDate);
        }
    }

    [Fact]
    public async Task GetAllReleasesAsync_ContainsExpectedVersions()
    {
        var releases = await _repository.GetAllReleasesAsync();
        var versions = releases.Select(r => r.ReleaseVersion).ToList();
        Assert.Contains("1.0.0", versions);
        Assert.Contains("1.1.0", versions);
        Assert.Contains("1.2.0", versions);
        Assert.Contains("2.0.0", versions);
        Assert.Contains("2.1.0", versions);
    }

    [Fact]
    public async Task CreateReleaseAsync_GeneratesIdWithRelPrefix()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "3.0.0" };
        var created = await _repository.CreateReleaseAsync(release);
        Assert.StartsWith("rel_", created.Id);
    }

    [Fact]
    public async Task CreateReleaseAsync_GeneratesIdWithCorrectLength()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "3.0.0" };
        var created = await _repository.CreateReleaseAsync(release);
        // "rel_" (4 chars) + 8 hex chars = 12 total
        Assert.Equal(12, created.Id.Length);
    }

    [Fact]
    public async Task CreateReleaseAsync_SetsCreatedDate()
    {
        var before = DateTime.UtcNow;
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "3.0.0" };
        var created = await _repository.CreateReleaseAsync(release);
        var after = DateTime.UtcNow;

        Assert.InRange(created.CreatedDate, before, after);
    }

    [Fact]
    public async Task CreateReleaseAsync_AddsToCollection()
    {
        var release = new Release { ReleaseName = "Test", ReleaseVersion = "3.0.0" };
        await _repository.CreateReleaseAsync(release);
        var all = await _repository.GetAllReleasesAsync();
        Assert.Equal(6, all.Count);
    }

    [Fact]
    public async Task CreateReleaseAsync_PreservesReleaseNameAndVersion()
    {
        var release = new Release { ReleaseName = "My Release", ReleaseVersion = "5.0.0" };
        var created = await _repository.CreateReleaseAsync(release);
        Assert.Equal("My Release", created.ReleaseName);
        Assert.Equal("5.0.0", created.ReleaseVersion);
    }

    [Fact]
    public async Task CreateReleaseAsync_GeneratesUniqueIds()
    {
        var release1 = new Release { ReleaseName = "R1", ReleaseVersion = "1.0.0" };
        var release2 = new Release { ReleaseName = "R2", ReleaseVersion = "2.0.0" };
        var created1 = await _repository.CreateReleaseAsync(release1);
        var created2 = await _repository.CreateReleaseAsync(release2);
        Assert.NotEqual(created1.Id, created2.Id);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WithExistingId_ReturnsUpdatedRelease()
    {
        var update = new Release
        {
            Id = "rel_001",
            ReleaseName = "Updated Release",
            ReleaseVersion = "1.0.1",
            AuthApiVersion = "1.0.1",
            ProductApiVersion = "1.0.1",
            UiAppVersion = "1.0.1",
            IsRolledBack = false
        };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WithExistingId_UpdatesReleaseName()
    {
        var update = new Release
        {
            Id = "rel_001",
            ReleaseName = "Updated Name",
            ReleaseVersion = "1.0.0",
            AuthApiVersion = "1.0.0",
            ProductApiVersion = "1.0.0",
            UiAppVersion = "1.0.0"
        };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.ReleaseName);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WithExistingId_UpdatesAllVersionFields()
    {
        var update = new Release
        {
            Id = "rel_001",
            ReleaseName = "Genesis Release",
            ReleaseVersion = "1.0.1",
            AuthApiVersion = "1.0.2",
            ProductApiVersion = "1.0.3",
            UiAppVersion = "1.0.4"
        };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.NotNull(result);
        Assert.Equal("1.0.1", result.ReleaseVersion);
        Assert.Equal("1.0.2", result.AuthApiVersion);
        Assert.Equal("1.0.3", result.ProductApiVersion);
        Assert.Equal("1.0.4", result.UiAppVersion);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WithExistingId_UpdatesIsRolledBack()
    {
        var update = new Release
        {
            Id = "rel_001",
            ReleaseName = "Genesis Release",
            ReleaseVersion = "1.0.0",
            AuthApiVersion = "1.0.0",
            ProductApiVersion = "1.0.0",
            UiAppVersion = "1.0.0",
            IsRolledBack = true
        };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.NotNull(result);
        Assert.True(result.IsRolledBack);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WithNonExistingId_ReturnsNull()
    {
        var update = new Release { Id = "rel_nonexistent", ReleaseName = "Test" };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateReleaseAsync_DoesNotChangeCreatedDate()
    {
        var allBefore = await _repository.GetAllReleasesAsync();
        var original = allBefore.First(r => r.Id == "rel_001");
        var originalDate = original.CreatedDate;

        var update = new Release
        {
            Id = "rel_001",
            ReleaseName = "Updated",
            ReleaseVersion = "1.0.1",
            AuthApiVersion = "1.0.1",
            ProductApiVersion = "1.0.1",
            UiAppVersion = "1.0.1"
        };
        var result = await _repository.UpdateReleaseAsync(update);
        Assert.NotNull(result);
        Assert.Equal(originalDate, result.CreatedDate);
    }
}
