using SilverOakGateClub.Models;

namespace SilverOakGateClub.ViewModel;

public class ResultViewModel
{
    public int ResultId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;

    public int Score { get; set; }
    public int TotalMarks { get; set; }
    public double Percentage { get; set; }

    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int Unanswered { get; set; }
    public int TotalQuestions { get; set; }

    public int TimeTakenSeconds { get; set; }
    public string TimeTakenFormatted => $"{TimeTakenSeconds / 60}m {TimeTakenSeconds % 60}s";

    public DateTime CompletedAt { get; set; }

    // For detailed review
    public List<QuestionReviewViewModel> QuestionReviews { get; set; } = new();
}

public class QuestionReviewViewModel
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    public int Marks { get; set; }
}

public class ResultHistoryViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public List<TestResult> Results { get; set; } = new();

    // Chart data
    public List<double> ScoreData { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public double OverallAverage { get; set; }
    public double BestScore { get; set; }
    public int TotalTestsTaken { get; set; }
}
