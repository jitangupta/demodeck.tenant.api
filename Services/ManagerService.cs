namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _managerRepository;

        public ManagerService(IManagerRepository managerRepository)
        {
            _managerRepository = managerRepository;
        }

        public async Task<List<Manager>> GetAllManagersAsync()
        {
            return await _managerRepository.GetAllManagersAsync();
        }

        public async Task<Manager?> ValidateCredentialsAsync(string username, string password)
        {
            var manager = await _managerRepository.GetManagerByUsernameAsync(username);
            if (manager == null) return null;

            var isValidPassword = await _managerRepository.ValidatePasswordAsync(manager, password);
            return isValidPassword ? manager : null;
        }
    }
}