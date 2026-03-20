using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetStudentsByBranchAsync(int branchId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string email);
    Task UpdateLastLoginAsync(int userId);
    Task<int> GetTotalStudentCountAsync();
    Task<List<User>> GetLeaderboardAsync(int branchId, int top = 10);
}
