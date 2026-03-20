using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(int id);
    Task<List<Branch>> GetAllActiveAsync();
    Task<List<Branch>> GetAllAsync();
    Task<Branch> CreateAsync(Branch branch);
    Task<Branch> UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
}
