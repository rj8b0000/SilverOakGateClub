using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class TestResult
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MockTestId { get; set; }
    public MockTest MockTest { get; set; } = null!;

    public int Score { get; set; }

    public int TotalMarks { get; set; }

    public int CorrectAnswers { get; set; }

    public int WrongAnswers { get; set; }

    public int Unanswered { get; set; }

    public double Percentage { get; set; }

    public int TimeTakenSeconds { get; set; }

    [MaxLength(4000)]
    public string? AnswersJson { get; set; } // JSON of user's answers

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
