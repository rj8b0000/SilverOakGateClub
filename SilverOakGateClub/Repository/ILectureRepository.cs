using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface ILectureRepository
{
    Task<Lecture?> GetByIdAsync(int id);
    Task<List<Lecture>> GetByBranchAsync(int branchId);
    Task<List<Lecture>> GetAllAsync();
    Task<Lecture> CreateAsync(Lecture lecture);
    Task<Lecture> UpdateAsync(Lecture lecture);
    Task DeleteAsync(int id);
    Task IncrementViewCountAsync(int id);
    Task<int> GetTotalCountAsync();
    Task<List<Lecture>> GetBySubjectAsync(int branchId, string subject);
    Task<List<string>> GetSubjectsByBranchAsync(int branchId);
    Task<List<Lecture>> GetByCreatorAsync(int userId);
}
