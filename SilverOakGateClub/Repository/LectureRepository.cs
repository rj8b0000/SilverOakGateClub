using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class LectureRepository : ILectureRepository
{
    private readonly ApplicationDbContext _context;

    public LectureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lecture?> GetByIdAsync(int id)
    {
        return await _context.Lectures
            .Include(l => l.Branch)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Lecture>> GetByBranchAsync(int branchId)
    {
        return await _context.Lectures
            .Where(l => (l.BranchId == branchId || l.BranchId == null) && l.IsActive)
            .Include(l => l.Branch)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<List<Lecture>> GetAllAsync()
    {
        return await _context.Lectures
            .Include(l => l.Branch)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<Lecture> CreateAsync(Lecture lecture)
    {
        _context.Lectures.Add(lecture);
        await _context.SaveChangesAsync();
        return lecture;
    }

    public async Task<Lecture> UpdateAsync(Lecture lecture)
    {
        _context.Lectures.Update(lecture);
        await _context.SaveChangesAsync();
        return lecture;
    }

    public async Task DeleteAsync(int id)
    {
        var lecture = await _context.Lectures.FindAsync(id);
        if (lecture != null)
        {
            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var lecture = await _context.Lectures.FindAsync(id);
        if (lecture != null)
        {
            lecture.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Lectures.CountAsync(l => l.IsActive);
    }

    public async Task<List<Lecture>> GetBySubjectAsync(int branchId, string subject)
    {
        return await _context.Lectures
            .Where(l => (l.BranchId == branchId || l.BranchId == null) && l.Subject == subject && l.IsActive)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<List<string>> GetSubjectsByBranchAsync(int branchId)
    {
        return await _context.Lectures
            .Where(l => (l.BranchId == branchId || l.BranchId == null) && l.IsActive && l.Subject != null)
            .Select(l => l.Subject!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }
}
