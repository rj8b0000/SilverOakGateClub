using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Models;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly ITestRepository _testRepo;
    private readonly ILectureRepository _lectureRepo;
    private readonly INotesRepository _notesRepo;
    private readonly IAnnouncementRepository _announcementRepo;
    private readonly IBranchRepository _branchRepo;
    private readonly IWebHostEnvironment _env;

    public AdminController(
        IUserRepository userRepo,
        ITestRepository testRepo,
        ILectureRepository lectureRepo,
        INotesRepository notesRepo,
        IAnnouncementRepository announcementRepo,
        IBranchRepository branchRepo,
        IWebHostEnvironment env)
    {
        _userRepo = userRepo;
        _testRepo = testRepo;
        _lectureRepo = lectureRepo;
        _notesRepo = notesRepo;
        _announcementRepo = announcementRepo;
        _branchRepo = branchRepo;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalStudents = await _userRepo.GetTotalStudentCountAsync(),
            TotalTests = await _testRepo.GetTotalTestsCountAsync(),
            TotalLectures = await _lectureRepo.GetTotalCountAsync(),
            TotalNotes = await _notesRepo.GetTotalCountAsync(),
            TotalQuestions = await _testRepo.GetTotalQuestionsCountAsync(),
            ActiveAnnouncements = (await _announcementRepo.GetActiveAsync()).Count,
            RecentUsers = (await _userRepo.GetAllAsync()).Take(5).ToList(),
            RecentResults = new()
        };
        return View(model);
    }

    // ── Users ──
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _userRepo.GetAllAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        var model = new CreateUserViewModel
        {
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        if (await _userRepo.ExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists.");
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password)));

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = hash,
            Role = model.Role,
            BranchId = model.BranchId,
            Phone = model.Phone,
            EnrollmentNumber = model.EnrollmentNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);
        TempData["Success"] = "User created successfully!";
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepo.DeleteAsync(id);
        TempData["Success"] = "User deleted successfully!";
        return RedirectToAction("Users");
    }

    // ── Tests ──
    [HttpGet]
    public async Task<IActionResult> Tests()
    {
        var tests = await _testRepo.GetAllTestsAsync();
        return View(tests);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTest()
    {
        var model = new CreateTestViewModel
        {
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTest(CreateTestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var test = new MockTest
        {
            Title = model.Title,
            Description = model.Description,
            BranchId = model.BranchId,
            DurationMinutes = model.DurationMinutes,
            IsPYQ = model.IsPYQ,
            Year = model.Year,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdTest = await _testRepo.CreateTestAsync(test);

        // Add questions
        if (model.Questions.Count > 0)
        {
            var questions = model.Questions.Select((q, i) => new Question
            {
                MockTestId = createdTest.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                Marks = q.Marks,
                NegativeMarks = q.NegativeMarks,
                Explanation = q.Explanation,
                OrderIndex = i + 1
            }).ToList();

            await _testRepo.AddQuestionsRangeAsync(questions);

            // Update totals
            createdTest.TotalQuestions = questions.Count;
            createdTest.TotalMarks = questions.Sum(q => q.Marks);
            await _testRepo.UpdateTestAsync(createdTest);
        }

        TempData["Success"] = "Test created successfully!";
        return RedirectToAction("Tests");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTest(int id)
    {
        await _testRepo.DeleteTestAsync(id);
        TempData["Success"] = "Test deleted successfully!";
        return RedirectToAction("Tests");
    }

    // ── Lectures ──
    [HttpGet]
    public async Task<IActionResult> Lectures()
    {
        var lectures = await _lectureRepo.GetAllAsync();
        return View(lectures);
    }

    [HttpGet]
    public async Task<IActionResult> CreateLecture()
    {
        var model = new CreateLectureViewModel
        {
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLecture(CreateLectureViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var lecture = new Lecture
        {
            Title = model.Title,
            Description = model.Description,
            VideoUrl = model.VideoUrl,
            ThumbnailUrl = model.ThumbnailUrl,
            BranchId = model.BranchId,
            Subject = model.Subject,
            DurationMinutes = model.DurationMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _lectureRepo.CreateAsync(lecture);
        TempData["Success"] = "Lecture created successfully!";
        return RedirectToAction("Lectures");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLecture(int id)
    {
        await _lectureRepo.DeleteAsync(id);
        TempData["Success"] = "Lecture deleted successfully!";
        return RedirectToAction("Lectures");
    }

    // ── Notes ──
    [HttpGet]
    public async Task<IActionResult> Notes()
    {
        var notes = await _notesRepo.GetAllAsync();
        return View(notes);
    }

    [HttpGet]
    public async Task<IActionResult> CreateNotes()
    {
        var model = new CreateNotesViewModel
        {
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNotes(CreateNotesViewModel model, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        string fileUrl = "";
        long fileSize = 0;
        string fileType = "PDF";

        if (file != null && file.Length > 0)
        {
            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", "Invalid file type. Allowed: PDF, DOC, DOCX, PPT, PPTX");
                model.Branches = await _branchRepo.GetAllActiveAsync();
                return View(model);
            }

            // Max 50MB
            if (file.Length > 50 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size must be less than 50MB.");
                model.Branches = await _branchRepo.GetAllActiveAsync();
                return View(model);
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "notes");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            fileUrl = $"/uploads/notes/{fileName}";
            fileSize = file.Length;
            fileType = ext.TrimStart('.').ToUpper();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var notes = new Notes
        {
            Title = model.Title,
            Description = model.Description,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSizeBytes = fileSize,
            BranchId = model.BranchId,
            Subject = model.Subject,
            UploadedByUserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _notesRepo.CreateAsync(notes);
        TempData["Success"] = "Notes uploaded successfully!";
        return RedirectToAction("Notes");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNotes(int id)
    {
        await _notesRepo.DeleteAsync(id);
        TempData["Success"] = "Notes deleted successfully!";
        return RedirectToAction("Notes");
    }

    // ── Announcements ──
    [HttpGet]
    public async Task<IActionResult> Announcements()
    {
        var announcements = await _announcementRepo.GetAllAsync();
        return View(announcements);
    }

    [HttpGet]
    public async Task<IActionResult> CreateAnnouncement()
    {
        var model = new CreateAnnouncementViewModel
        {
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var announcement = new Announcement
        {
            Title = model.Title,
            Content = model.Content,
            Priority = model.Priority,
            BranchId = model.BranchId,
            CreatedByUserId = userId,
            ExpiresAt = model.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _announcementRepo.CreateAsync(announcement);
        TempData["Success"] = "Announcement posted successfully!";
        return RedirectToAction("Announcements");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        await _announcementRepo.DeleteAsync(id);
        TempData["Success"] = "Announcement deleted successfully!";
        return RedirectToAction("Announcements");
    }
}
