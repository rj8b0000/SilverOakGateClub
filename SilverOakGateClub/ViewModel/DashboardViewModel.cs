using SilverOakGateClub.Models;

namespace SilverOakGateClub.ViewModel;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string Role { get; set; } = string.Empty;

    // Stats
    public int TotalTests { get; set; }
    public int TestsAttempted { get; set; }
    public double AverageScore { get; set; }
    public int TotalLectures { get; set; }
    public int TotalNotes { get; set; }
    public int TotalStudents { get; set; }
    public int TotalQuestions { get; set; }

    // Recent data
    public List<TestResult> RecentResults { get; set; } = new();
    public List<Announcement> RecentAnnouncements { get; set; } = new();
    public List<MockTest> UpcomingTests { get; set; } = new();

    // Leaderboard
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();

    // Chart data
    public List<double> ScoreHistory { get; set; } = new();
    public List<string> ScoreLabels { get; set; } = new();
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int TestsAttempted { get; set; }
    public string? ProfileImageUrl { get; set; }
}
