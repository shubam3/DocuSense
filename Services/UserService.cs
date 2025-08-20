using DocuSense.Data;
using DocuSense.Models;
using DocuSense.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DocuSense.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _userManager.Users
                    .OrderBy(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password)
        {
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                _logger.Information("User created successfully: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating user: {Email}", user.Email);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.Information("User updated successfully: {UserId}", user.Id);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.Information("User deleted successfully: {UserId}", userId);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                return await _userManager.IsInRoleAsync(user, role);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if user {UserId} is in role {Role}", userId, role);
                throw;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new List<string>();
                }

                return (await _userManager.GetRolesAsync(user)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting roles for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    _logger.Information("User {UserId} added to role {Role}", userId, role);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding user {UserId} to role {Role}", userId, role);
                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    _logger.Information("User {UserId} removed from role {Role}", userId, role);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing user {UserId} from role {Role}", userId, role);
                throw;
            }
        }

        public async Task<int> GetUserCountAsync()
        {
            try
            {
                return await _userManager.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user count");
                throw;
            }
        }

        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                return usersInRole.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting users in role: {Role}", role);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.LastLoginAt = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating last login for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> LockUserAsync(string userId, DateTime lockoutEnd)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                if (result.Succeeded)
                {
                    _logger.Information("User {UserId} locked until {LockoutEnd}", userId, lockoutEnd);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error locking user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (result.Succeeded)
                {
                    _logger.Information("User {UserId} unlocked", userId);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unlocking user: {UserId}", userId);
                throw;
            }
        }
    }
} 