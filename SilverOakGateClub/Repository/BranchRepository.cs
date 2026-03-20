using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class BranchRepository : IBranchRepository
{
    private readonly ApplicationDbContext _context;

    public BranchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches.FindAsync(id);
    }

    public async Task<List<Branch>> GetAllActiveAsync()
    {
        return await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<List<Branch>> GetAllAsync()
    {
        return await _context.Branches
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Branch> CreateAsync(Branch branch)
    {
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return branch;
    }

    public async Task<Branch> UpdateAsync(Branch branch)
    {
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();
        return branch;
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
        }
    }
}
