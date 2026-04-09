using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface INotesRepository
{
    Task<Notes?> GetByIdAsync(int id);
    Task<List<Notes>> GetByBranchAsync(int branchId);
    Task<List<Notes>> GetAllAsync();
    Task<Notes> CreateAsync(Notes notes);
    Task<Notes> UpdateAsync(Notes notes);
    Task DeleteAsync(int id);
    Task IncrementDownloadCountAsync(int id);
    Task UpdateRatingAsync(int id, double newRating);
    Task<int> GetTotalCountAsync();
    Task<List<Notes>> GetBySubjectAsync(int branchId, string subject);
    Task<List<string>> GetSubjectsByBranchAsync(int branchId);
    Task<List<Notes>> GetTopRatedAsync(int branchId, int top = 10);
    Task<List<Notes>> GetByUploaderAsync(int userId);
}
