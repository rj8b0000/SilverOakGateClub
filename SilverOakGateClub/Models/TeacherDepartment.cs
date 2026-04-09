namespace SilverOakGateClub.Models;

public class TeacherDepartment
{
    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Branch Department { get; set; } = null!;
}
