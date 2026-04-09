using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class NotesRepository : INotesRepository
{
    private readonly ApplicationDbContext _context;

    public NotesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notes?> GetByIdAsync(int id)
    {
        return await _context.Notes
            .Include(n => n.Branch)
            .Include(n => n.UploadedBy)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Notes>> GetByBranchAsync(int branchId)
    {
        return await _context.Notes
            .Where(n => (n.BranchId == branchId || n.BranchId == null) && n.IsActive)
            .Include(n => n.Branch)
            .Include(n => n.UploadedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notes>> GetAllAsync()
    {
        return await _context.Notes
            .Include(n => n.Branch)
            .Include(n => n.UploadedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notes> CreateAsync(Notes notes)
    {
        _context.Notes.Add(notes);
        await _context.SaveChangesAsync();
        return notes;
    }

    public async Task<Notes> UpdateAsync(Notes notes)
    {
        _context.Notes.Update(notes);
        await _context.SaveChangesAsync();
        return notes;
    }

    public async Task DeleteAsync(int id)
    {
        var notes = await _context.Notes.FindAsync(id);
        if (notes != null)
        {
            _context.Notes.Remove(notes);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementDownloadCountAsync(int id)
    {
        var notes = await _context.Notes.FindAsync(id);
        if (notes != null)
        {
            notes.DownloadCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateRatingAsync(int id, double newRating)
    {
        var notes = await _context.Notes.FindAsync(id);
        if (notes != null)
        {
            notes.AverageRating = ((notes.AverageRating * notes.RatingCount) + newRating) / (notes.RatingCount + 1);
            notes.RatingCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Notes.CountAsync(n => n.IsActive);
    }

    public async Task<List<Notes>> GetBySubjectAsync(int branchId, string subject)
    {
        return await _context.Notes
            .Where(n => (n.BranchId == branchId || n.BranchId == null) && n.Subject == subject && n.IsActive)
            .Include(n => n.UploadedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<string>> GetSubjectsByBranchAsync(int branchId)
    {
        return await _context.Notes
            .Where(n => (n.BranchId == branchId || n.BranchId == null) && n.IsActive && n.Subject != null)
            .Select(n => n.Subject!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<Notes>> GetTopRatedAsync(int branchId, int top = 10)
    {
        return await _context.Notes
            .Where(n => (n.BranchId == branchId || n.BranchId == null) && n.IsActive)
            .Include(n => n.UploadedBy)
            .OrderByDescending(n => n.AverageRating)
            .Take(top)
            .ToListAsync();
    }

    public async Task<List<Notes>> GetByUploaderAsync(int userId)
    {
        return await _context.Notes
            .Where(n => n.UploadedByUserId == userId)
            .Include(n => n.Branch)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
