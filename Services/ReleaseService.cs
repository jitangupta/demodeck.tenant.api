namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class ReleaseService : IReleaseService
    {
        private readonly IReleaseRepository _releaseRepository;

        public ReleaseService(IReleaseRepository releaseRepository)
        {
            _releaseRepository = releaseRepository;
        }

        public async Task<List<Release>> GetAllReleasesAsync()
        {
            return await _releaseRepository.GetAllReleasesAsync();
        }

        public async Task<Release> CreateReleaseAsync(Release release)
        {
            return await _releaseRepository.CreateReleaseAsync(release);
        }

        public async Task<Release?> UpdateReleaseAsync(Release release)
        {
            return await _releaseRepository.UpdateReleaseAsync(release);
        }
    }
}