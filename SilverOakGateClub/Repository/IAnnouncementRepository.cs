using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(int id);
    Task<List<Announcement>> GetActiveAsync(int? branchId = null);
    Task<List<Announcement>> GetAllAsync();
    Task<Announcement> CreateAsync(Announcement announcement);
    Task<Announcement> UpdateAsync(Announcement announcement);
    Task DeleteAsync(int id);
}
