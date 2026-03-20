using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class MockTest
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int DurationMinutes { get; set; } = 60;

    public int TotalMarks { get; set; }

    public int TotalQuestions { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPYQ { get; set; } = false; // Previous Year Question paper

    public int? Year { get; set; } // For PYQ papers

    [MaxLength(500)]
    public string? PdfUrl { get; set; } // For PYQ PDFs

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
