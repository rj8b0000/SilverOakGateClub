using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Models;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

[Authorize]
public class TestController : Controller
{
    private readonly ITestRepository _testRepo;

    public TestController(ITestRepository testRepo)
    {
        _testRepo = testRepo;
    }

    // List all mock tests
    public async Task<IActionResult> Index()
    {
        var branchId = GetBranchId();
        if (branchId == null) return RedirectToAction("SelectBranch", "Auth");

        var tests = await _testRepo.GetTestsByBranchAsync(branchId.Value);
        var model = new TestListViewModel
        {
            BranchName = User.FindFirstValue("BranchName") ?? "",
            BranchId = branchId.Value,
            Tests = tests,
            IsPYQ = false
        };
        return View(model);
    }

    // PYQ listing
    public async Task<IActionResult> PYQ()
    {
        var branchId = GetBranchId();
        if (branchId == null) return RedirectToAction("SelectBranch", "Auth");

        var pyqs = await _testRepo.GetPYQsByBranchAsync(branchId.Value);
        var model = new TestListViewModel
        {
            BranchName = User.FindFirstValue("BranchName") ?? "",
            BranchId = branchId.Value,
            Tests = pyqs,
            IsPYQ = true
        };
        return View("Index", model);
    }

    // Start a test
    [HttpGet]
    public async Task<IActionResult> Start(int id)
    {
        var test = await _testRepo.GetTestWithQuestionsAsync(id);
        if (test == null) return NotFound();

        var model = new TestSubmissionViewModel
        {
            TestId = test.Id,
            TestTitle = test.Title,
            DurationMinutes = test.DurationMinutes,
            TotalQuestions = test.Questions.Count,
            TotalMarks = test.Questions.Sum(q => q.Marks),
            BranchName = test.Branch.Name,
            Questions = test.Questions.Select((q, i) => new QuestionViewModel
            {
                QuestionId = q.Id,
                QuestionNumber = i + 1,
                QuestionText = q.QuestionText,
                ImageUrl = q.ImageUrl,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                Marks = q.Marks,
                NegativeMarks = q.NegativeMarks
            }).ToList()
        };

        return View(model);
    }

    // Submit test answers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit([FromBody] TestAnswerSubmission submission)
    {
        var test = await _testRepo.GetTestWithQuestionsAsync(submission.TestId);
        if (test == null) return NotFound();

        var userId = GetUserId();
        int correct = 0, wrong = 0, unanswered = 0, score = 0;

        foreach (var question in test.Questions)
        {
            if (submission.Answers.TryGetValue(question.Id, out var answer) && !string.IsNullOrEmpty(answer))
            {
                if (answer == question.CorrectAnswer)
                {
                    correct++;
                    score += question.Marks;
                }
                else
                {
                    wrong++;
                    score -= question.NegativeMarks;
                }
            }
            else
            {
                unanswered++;
            }
        }

        var totalMarks = test.Questions.Sum(q => q.Marks);
        var percentage = totalMarks > 0 ? Math.Round((double)score / totalMarks * 100, 2) : 0;
        if (percentage < 0) percentage = 0;

        var result = new TestResult
        {
            UserId = userId,
            MockTestId = test.Id,
            Score = Math.Max(score, 0),
            TotalMarks = totalMarks,
            CorrectAnswers = correct,
            WrongAnswers = wrong,
            Unanswered = unanswered,
            Percentage = percentage,
            TimeTakenSeconds = submission.TimeTakenSeconds,
            AnswersJson = JsonSerializer.Serialize(submission.Answers),
            CompletedAt = DateTime.UtcNow
        };

        await _testRepo.SaveResultAsync(result);

        return Json(new { resultId = result.Id });
    }

    // View result
    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        var result = await _testRepo.GetResultByIdAsync(id);
        if (result == null) return NotFound();

        var test = await _testRepo.GetTestWithQuestionsAsync(result.MockTestId);
        var userAnswers = !string.IsNullOrEmpty(result.AnswersJson)
            ? JsonSerializer.Deserialize<Dictionary<int, string>>(result.AnswersJson)
            : new Dictionary<int, string>();

        var model = new ResultViewModel
        {
            ResultId = result.Id,
            TestTitle = result.MockTest.Title,
            BranchName = result.MockTest.Branch?.Name ?? "",
            StudentName = result.User.FullName,
            Score = result.Score,
            TotalMarks = result.TotalMarks,
            Percentage = result.Percentage,
            CorrectAnswers = result.CorrectAnswers,
            WrongAnswers = result.WrongAnswers,
            Unanswered = result.Unanswered,
            TotalQuestions = result.CorrectAnswers + result.WrongAnswers + result.Unanswered,
            TimeTakenSeconds = result.TimeTakenSeconds,
            CompletedAt = result.CompletedAt,
            QuestionReviews = test?.Questions.Select((q, i) => new QuestionReviewViewModel
            {
                QuestionNumber = i + 1,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                UserAnswer = userAnswers?.GetValueOrDefault(q.Id),
                IsCorrect = userAnswers?.GetValueOrDefault(q.Id) == q.CorrectAnswer,
                Explanation = q.Explanation,
                Marks = q.Marks
            }).ToList() ?? new()
        };

        return View(model);
    }

    // Result History
    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        var results = await _testRepo.GetResultsByUserAsync(userId);

        var model = new ResultHistoryViewModel
        {
            StudentName = User.Identity?.Name ?? "",
            BranchName = User.FindFirstValue("BranchName") ?? "",
            Results = results,
            ScoreData = results.OrderBy(r => r.CompletedAt).Select(r => r.Percentage).ToList(),
            Labels = results.OrderBy(r => r.CompletedAt).Select(r => r.MockTest.Title).ToList(),
            OverallAverage = results.Count > 0 ? Math.Round(results.Average(r => r.Percentage), 1) : 0,
            BestScore = results.Count > 0 ? results.Max(r => r.Percentage) : 0,
            TotalTestsTaken = results.Count
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
