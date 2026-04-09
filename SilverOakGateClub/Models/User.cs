using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Student"; // "Admin", "Teacher", or "Student"

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? EnrollmentNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<TeacherDepartment> TeacherDepartments { get; set; } = new List<TeacherDepartment>();
}
