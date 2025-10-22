namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class InMemoryReleaseRepository : IReleaseRepository
    {
        private readonly List<Release> _releases = new();

        public InMemoryReleaseRepository()
        {
            SeedData();
        }

        private void SeedData()
        {
            _releases.AddRange(new[]
            {
                new Release
                {
                    Id = "rel_001",
                    ReleaseName = "Genesis Release",
                    ReleaseVersion = "1.0.0",
                    AuthApiVersion = "1.0.0",
                    ProductApiVersion = "1.0.0", 
                    UiAppVersion = "1.0.0",
                    CreatedDate = DateTime.UtcNow.AddDays(-45)
                },
                new Release
                {
                    Id = "rel_002",
                    ReleaseName = "Feature Enhancement",
                    ReleaseVersion = "1.1.0",
                    AuthApiVersion = "1.1.0",
                    ProductApiVersion = "1.1.0",
                    UiAppVersion = "1.1.0", 
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                },
                new Release
                {
                    Id = "rel_003",
                    ReleaseName = "Performance Boost",
                    ReleaseVersion = "1.2.0",
                    AuthApiVersion = "1.2.0",
                    ProductApiVersion = "1.2.0",
                    UiAppVersion = "1.2.0", 
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                },
                new Release
                {
                    Id = "rel_004",
                    ReleaseName = "Yarp Implementation",
                    ReleaseVersion = "2.0.0",
                    AuthApiVersion = "2.0.0",
                    ProductApiVersion = "2.0.0",
                    UiAppVersion = "2.0.0", 
                    CreatedDate = DateTime.UtcNow.AddDays(-7)
                },
                new Release
                {
                    Id = "rel_005",
                    ReleaseName = "Task Feature Update",
                    ReleaseVersion = "2.1.0",
                    AuthApiVersion = "2.1.0",
                    ProductApiVersion = "2.1.0",
                    UiAppVersion = "2.1.0",
                    CreatedDate = DateTime.UtcNow.AddDays(-7)
                }
            });
        }

        public Task<List<Release>> GetAllReleasesAsync()
        {
            return Task.FromResult(_releases.OrderByDescending(r => r.CreatedDate).ToList());
        }

        public Task<Release> CreateReleaseAsync(Release release)
        {
            release.Id = $"rel_{Guid.NewGuid().ToString("N")[..8]}";
            release.CreatedDate = DateTime.UtcNow;
            _releases.Add(release);
            return Task.FromResult(release);
        }

        public Task<Release?> UpdateReleaseAsync(Release release)
        {
            var existing = _releases.FirstOrDefault(r => r.Id == release.Id);
            if (existing == null) return Task.FromResult<Release?>(null);

            existing.ReleaseName = release.ReleaseName;
            existing.ReleaseVersion = release.ReleaseVersion;
            existing.AuthApiVersion = release.AuthApiVersion;
            existing.ProductApiVersion = release.ProductApiVersion;
            existing.UiAppVersion = release.UiAppVersion;
            existing.IsRolledBack = release.IsRolledBack;

            return Task.FromResult<Release?>(existing);
        }
    }
}