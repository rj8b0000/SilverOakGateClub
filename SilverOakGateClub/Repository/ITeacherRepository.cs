using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface ITeacherRepository
{
    Task<List<User>> GetAllTeachersAsync();
    Task<User?> GetTeacherWithDepartmentsAsync(int userId);
    Task AssignDepartmentsAsync(int teacherId, List<int> departmentIds);
    Task<List<Branch>> GetAssignedDepartmentsAsync(int teacherId);
    Task<bool> IsTeacherAssignedToDepartmentAsync(int teacherId, int departmentId);
    Task<int> GetTotalTeacherCountAsync();
}
