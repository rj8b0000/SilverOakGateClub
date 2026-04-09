using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class TeacherRepository : ITeacherRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllTeachersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == "Teacher")
            .Include(u => u.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetTeacherWithDepartmentsAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.TeacherDepartments)
                .ThenInclude(td => td.Department)
            .FirstOrDefaultAsync(u => u.Id == userId && u.Role == "Teacher");
    }

    public async Task AssignDepartmentsAsync(int teacherId, List<int> departmentIds)
    {
        // Remove existing assignments
        var existing = await _context.TeacherDepartments
            .Where(td => td.TeacherId == teacherId)
            .ToListAsync();
        _context.TeacherDepartments.RemoveRange(existing);

        // Add new assignments
        var newAssignments = departmentIds.Select(deptId => new TeacherDepartment
        {
            TeacherId = teacherId,
            DepartmentId = deptId
        });
        _context.TeacherDepartments.AddRange(newAssignments);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Branch>> GetAssignedDepartmentsAsync(int teacherId)
    {
        return await _context.TeacherDepartments
            .Where(td => td.TeacherId == teacherId)
            .Select(td => td.Department)
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<bool> IsTeacherAssignedToDepartmentAsync(int teacherId, int departmentId)
    {
        return await _context.TeacherDepartments
            .AnyAsync(td => td.TeacherId == teacherId && td.DepartmentId == departmentId);
    }

    public async Task<int> GetTotalTeacherCountAsync()
    {
        return await _context.Users.CountAsync(u => u.Role == "Teacher" && u.IsActive);
    }
}
