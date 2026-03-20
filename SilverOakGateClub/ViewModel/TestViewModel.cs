using SilverOakGateClub.Models;

namespace SilverOakGateClub.ViewModel;

public class TestSubmissionViewModel
{
    public int TestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalMarks { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public int Marks { get; set; }
    public int NegativeMarks { get; set; }
    public string? SelectedAnswer { get; set; }
}

public class TestAnswerSubmission
{
    public int TestId { get; set; }
    public int TimeTakenSeconds { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new(); // QuestionId -> Answer
}

public class TestListViewModel
{
    public string BranchName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public List<MockTest> Tests { get; set; } = new();
    public bool IsPYQ { get; set; }
}
