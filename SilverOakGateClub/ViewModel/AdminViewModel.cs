using System.ComponentModel.DataAnnotations;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.ViewModel;

// Admin ViewModels
public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalTests { get; set; }
    public int TotalLectures { get; set; }
    public int TotalNotes { get; set; }
    public int TotalQuestions { get; set; }
    public int ActiveAnnouncements { get; set; }
    public List<User> RecentUsers { get; set; } = new();
    public List<TestResult> RecentResults { get; set; } = new();
}

public class CreateUserViewModel
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    public int? BranchId { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? EnrollmentNumber { get; set; }

    public List<Branch> Branches { get; set; } = new();
}

public class CreateTestViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? BranchId { get; set; }

    [Required, Range(1, 300)]
    public int DurationMinutes { get; set; } = 60;

    public bool IsPYQ { get; set; }

    public int? Year { get; set; }

    public List<CreateQuestionViewModel> Questions { get; set; } = new();

    public List<Branch> Branches { get; set; } = new();
}

public class CreateQuestionViewModel
{
    [Required]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    public string OptionD { get; set; } = string.Empty;

    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Marks { get; set; } = 1;

    [Range(0, 5)]
    public int NegativeMarks { get; set; } = 0;

    public string? Explanation { get; set; }
}

public class CreateLectureViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(500)]
    public string VideoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? BranchId { get; set; }

    [MaxLength(100)]
    public string? Subject { get; set; }

    public int DurationMinutes { get; set; }

    public List<Branch> Branches { get; set; } = new();
}

public class CreateNotesViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? BranchId { get; set; }

    [MaxLength(100)]
    public string? Subject { get; set; }

    public List<Branch> Branches { get; set; } = new();
}

public class CreateAnnouncementViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";

    public int? BranchId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public List<Branch> Branches { get; set; } = new();
}

public class BranchSelectionViewModel
{
    public List<Branch> Branches { get; set; } = new();
    public int? SelectedBranchId { get; set; }
}

public class LectureListViewModel
{
    public string BranchName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public List<Lecture> Lectures { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public string? SelectedSubject { get; set; }
}

public class NotesListViewModel
{
    public string BranchName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public List<Notes> NotesList { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public string? SelectedSubject { get; set; }
}
