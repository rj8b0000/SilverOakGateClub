using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly ApplicationDbContext _context;

    public AnnouncementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Announcement?> GetByIdAsync(int id)
    {
        return await _context.Announcements
            .Include(a => a.CreatedBy)
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Announcement>> GetActiveAsync(int? branchId = null)
    {
        var query = _context.Announcements
            .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow));

        if (branchId.HasValue)
        {
            query = query.Where(a => a.BranchId == null || a.BranchId == branchId.Value);
        }

        return await query
            .Include(a => a.CreatedBy)
            .OrderByDescending(a => a.Priority == "Urgent")
            .ThenByDescending(a => a.Priority == "High")
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Announcement>> GetAllAsync()
    {
        return await _context.Announcements
            .Include(a => a.CreatedBy)
            .Include(a => a.Branch)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement> CreateAsync(Announcement announcement)
    {
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task<Announcement> UpdateAsync(Announcement announcement)
    {
        _context.Announcements.Update(announcement);
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task DeleteAsync(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
        }
    }
}
