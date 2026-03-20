using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly ITestRepository _testRepo;
    private readonly ILectureRepository _lectureRepo;
    private readonly INotesRepository _notesRepo;
    private readonly IAnnouncementRepository _announcementRepo;

    public DashboardController(
        IUserRepository userRepo,
        ITestRepository testRepo,
        ILectureRepository lectureRepo,
        INotesRepository notesRepo,
        IAnnouncementRepository announcementRepo)
    {
        _userRepo = userRepo;
        _testRepo = testRepo;
        _lectureRepo = lectureRepo;
        _notesRepo = notesRepo;
        _announcementRepo = announcementRepo;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var branchId = GetBranchId();

        if (branchId == null)
            return RedirectToAction("SelectBranch", "Auth");

        var user = await _userRepo.GetByIdAsync(userId);
        var results = await _testRepo.GetResultsByUserAsync(userId);
        var announcements = await _announcementRepo.GetActiveAsync(branchId);
        var tests = await _testRepo.GetTestsByBranchAsync(branchId.Value);

        // Build leaderboard
        var leaderboardUsers = await _userRepo.GetLeaderboardAsync(branchId.Value);
        var leaderboard = leaderboardUsers.Select((u, i) => new LeaderboardEntry
        {
            Rank = i + 1,
            StudentName = u.FullName,
            AverageScore = u.TestResults.Count > 0 ? Math.Round(u.TestResults.Average(r => r.Percentage), 1) : 0,
            TestsAttempted = u.TestResults.Count,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToList();

        var model = new DashboardViewModel
        {
            UserName = user?.FullName ?? "",
            BranchName = User.FindFirstValue("BranchName") ?? "",
            BranchId = branchId.Value,
            Role = User.FindFirstValue(ClaimTypes.Role) ?? "Student",
            TotalTests = tests.Count,
            TestsAttempted = results.Count,
            AverageScore = results.Count > 0 ? Math.Round(results.Average(r => r.Percentage), 1) : 0,
            TotalLectures = await _lectureRepo.GetTotalCountAsync(),
            TotalNotes = await _notesRepo.GetTotalCountAsync(),
            TotalStudents = await _userRepo.GetTotalStudentCountAsync(),
            TotalQuestions = await _testRepo.GetTotalQuestionsCountAsync(),
            RecentResults = results.Take(5).ToList(),
            RecentAnnouncements = announcements.Take(5).ToList(),
            UpcomingTests = tests.Take(5).ToList(),
            Leaderboard = leaderboard,
            ScoreHistory = results.OrderBy(r => r.CompletedAt).Select(r => r.Percentage).ToList(),
            ScoreLabels = results.OrderBy(r => r.CompletedAt).Select(r => r.MockTest.Title).ToList()
        };

        return View(model);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private int? GetBranchId()
    {
        var claim = User.FindFirstValue("BranchId");
        return claim != null ? int.Parse(claim) : null;
    }
}
