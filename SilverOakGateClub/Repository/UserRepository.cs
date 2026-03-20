using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Branch)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<List<User>> GetStudentsByBranchAsync(int branchId)
    {
        return await _context.Users
            .Where(u => u.BranchId == branchId && u.Role == "Student")
            .Include(u => u.Branch)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalStudentCountAsync()
    {
        return await _context.Users.CountAsync(u => u.Role == "Student");
    }

    public async Task<List<User>> GetLeaderboardAsync(int branchId, int top = 10)
    {
        return await _context.Users
            .Where(u => u.BranchId == branchId && u.Role == "Student")
            .Include(u => u.TestResults)
            .OrderByDescending(u => u.TestResults.Average(tr => tr.Percentage))
            .Take(top)
            .ToListAsync();
    }
}
