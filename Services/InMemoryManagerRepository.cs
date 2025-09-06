namespace Demodeck.Tenant.Api.Services
{
    using Demodeck.Tenant.Api.Models;

    public class InMemoryManagerRepository : IManagerRepository
    {
        private readonly List<Manager> _managers = new();

        public InMemoryManagerRepository()
        {
            SeedData();
        }

        private void SeedData()
        {
            _managers.AddRange(new[]
            {
                new Manager
                {
                    Id = "mgr_001",
                    Username = "admin",
                    Email = "admin@demodeck.xyz",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "SuperAdmin",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Manager
                {
                    Id = "mgr_002",
                    Username = "manager",
                    Email = "manager@demodeck.xyz", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                }
            });
        }

        public Task<List<Manager>> GetAllManagersAsync()
        {
            return Task.FromResult(_managers.Where(m => m.IsActive).ToList());
        }

        public Task<Manager?> GetManagerByUsernameAsync(string username)
        {
            var manager = _managers.FirstOrDefault(m => 
                m.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && m.IsActive);
            return Task.FromResult(manager);
        }

        public Task<bool> ValidatePasswordAsync(Manager manager, string password)
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, manager.PasswordHash);
            return Task.FromResult(isValid);
        }
    }
}