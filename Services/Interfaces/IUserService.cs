using DocuSense.Models;

namespace DocuSense.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<List<ApplicationUser>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 50);
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> IsUserInRoleAsync(string userId, string role);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> AddUserToRoleAsync(string userId, string role);
        Task<bool> RemoveUserFromRoleAsync(string userId, string role);
        Task<int> GetUserCountAsync();
        Task<List<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task<bool> UpdateLastLoginAsync(string userId);
        Task<bool> LockUserAsync(string userId, DateTime lockoutEnd);
        Task<bool> UnlockUserAsync(string userId);
    }
} 