using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
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
    private readonly ITeacherRepository _teacherRepo;
    private readonly IWebHostEnvironment _env;

    public AdminController(
        IUserRepository userRepo,
        ITestRepository testRepo,
        ILectureRepository lectureRepo,
        INotesRepository notesRepo,
        IAnnouncementRepository announcementRepo,
        IBranchRepository branchRepo,
        ITeacherRepository teacherRepo,
        IWebHostEnvironment env)
    {
        _userRepo = userRepo;
        _testRepo = testRepo;
        _lectureRepo = lectureRepo;
        _notesRepo = notesRepo;
        _announcementRepo = announcementRepo;
        _branchRepo = branchRepo;
        _teacherRepo = teacherRepo;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalStudents = await _userRepo.GetTotalStudentCountAsync(),
            TotalTeachers = await _teacherRepo.GetTotalTeacherCountAsync(),
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

    // ══════════════════════════════════
    //  USERS
    // ══════════════════════════════════
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

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId,
            Phone = user.Phone,
            EnrollmentNumber = user.EnrollmentNumber,
            IsActive = user.IsActive,
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var user = await _userRepo.GetByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Role = model.Role;
        user.BranchId = model.BranchId;
        user.Phone = model.Phone;
        user.EnrollmentNumber = model.EnrollmentNumber;
        user.IsActive = model.IsActive;

        await _userRepo.UpdateAsync(user);
        TempData["Success"] = "User updated successfully!";
        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> ViewUser(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepo.DeleteAsync(id);
        TempData["Success"] = "User deleted successfully!";
        return RedirectToAction("Users");
    }

    // ══════════════════════════════════
    //  TEACHERS
    // ══════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Teachers()
    {
        var teachers = await _teacherRepo.GetAllTeachersAsync();
        return View(teachers);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTeacher()
    {
        var model = new CreateTeacherViewModel
        {
            AllDepartments = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllDepartments = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        if (await _userRepo.ExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists.");
            model.AllDepartments = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password)));

        var teacher = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = hash,
            Role = "Teacher",
            Phone = model.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(teacher);

        // Assign departments
        if (model.SelectedDepartmentIds.Count > 0)
        {
            await _teacherRepo.AssignDepartmentsAsync(teacher.Id, model.SelectedDepartmentIds);
        }

        TempData["Success"] = "Teacher created and departments assigned successfully!";
        return RedirectToAction("Teachers");
    }

    [HttpGet]
    public async Task<IActionResult> EditTeacher(int id)
    {
        var teacher = await _teacherRepo.GetTeacherWithDepartmentsAsync(id);
        if (teacher == null) return NotFound();

        var model = new EditTeacherViewModel
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            IsActive = teacher.IsActive,
            SelectedDepartmentIds = teacher.TeacherDepartments.Select(td => td.DepartmentId).ToList(),
            AllDepartments = await _branchRepo.GetAllActiveAsync(),
            AssignedDepartments = teacher.TeacherDepartments.Select(td => td.Department).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTeacher(EditTeacherViewModel model)
    {
        var teacher = await _userRepo.GetByIdAsync(model.TeacherId);
        if (teacher == null) return NotFound();

        teacher.FullName = model.FullName;
        teacher.Phone = model.Phone;
        teacher.IsActive = model.IsActive;
        await _userRepo.UpdateAsync(teacher);

        await _teacherRepo.AssignDepartmentsAsync(model.TeacherId, model.SelectedDepartmentIds);

        TempData["Success"] = "Teacher updated successfully!";
        return RedirectToAction("Teachers");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateTeacher(int id)
    {
        var teacher = await _userRepo.GetByIdAsync(id);
        if (teacher != null)
        {
            teacher.IsActive = false;
            await _userRepo.UpdateAsync(teacher);
        }
        TempData["Success"] = "Teacher deactivated successfully!";
        return RedirectToAction("Teachers");
    }

    // ══════════════════════════════════
    //  TESTS
    // ══════════════════════════════════
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

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var test = new MockTest
        {
            Title = model.Title,
            Description = model.Description,
            BranchId = model.BranchId,
            DurationMinutes = model.DurationMinutes,
            IsPYQ = model.IsPYQ,
            Year = model.Year,
            CreatedByUserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdTest = await _testRepo.CreateTestAsync(test);

        List<Question> questions = new();

        // 1. Handle Excel Upload if provided
        if (model.ExcelFile != null && model.ExcelFile.Length > 0)
        {
            var (excelQuestions, errors) = await ParseQuestionsFromExcel(model.ExcelFile, createdTest.Id);
            if (errors.Any())
            {
                foreach (var err in errors) ModelState.AddModelError("", err);
                await _testRepo.DeleteTestAsync(createdTest.Id); // Cleanup
                model.Branches = await _branchRepo.GetAllActiveAsync();
                return View(model);
            }
            questions = excelQuestions;
        }
        // 2. Otherwise handle Manual Entry if provided
        else if (model.Questions != null && model.Questions.Count > 0)
        {
            questions = model.Questions.Select((q, i) => new Question
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
        }

        if (questions.Count > 0)
        {
            await _testRepo.AddQuestionsRangeAsync(questions);

            // Update totals
            createdTest.TotalQuestions = questions.Count;
            createdTest.TotalMarks = questions.Sum(q => q.Marks);
            await _testRepo.UpdateTestAsync(createdTest);
        }

        TempData["Success"] = "Test created successfully!";
        return RedirectToAction("Tests");
    }

    [HttpGet]
    public async Task<IActionResult> EditTest(int id)
    {
        var test = await _testRepo.GetTestWithQuestionsAsync(id);
        if (test == null) return NotFound();

        var model = new EditTestViewModel
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            BranchId = test.BranchId,
            DurationMinutes = test.DurationMinutes,
            IsPYQ = test.IsPYQ,
            Year = test.Year,
            IsActive = test.IsActive,
            Branches = await _branchRepo.GetAllActiveAsync(),
            Questions = test.Questions.Select(q => new CreateQuestionViewModel
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                Marks = q.Marks,
                NegativeMarks = q.NegativeMarks,
                Explanation = q.Explanation
            }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTest(EditTestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var test = await _testRepo.GetTestWithQuestionsAsync(model.Id);
        if (test == null) return NotFound();

        test.Title = model.Title;
        test.Description = model.Description;
        test.BranchId = model.BranchId;
        test.DurationMinutes = model.DurationMinutes;
        test.IsPYQ = model.IsPYQ;
        test.Year = model.Year;
        test.IsActive = model.IsActive;

        // --- Question Sync Logic ---
        var incomingIds = model.Questions.Where(q => q.Id.HasValue).Select(q => q.Id!.Value).ToList();
        
        // 1. Delete removed questions
        var toDelete = test.Questions.Where(q => !incomingIds.Contains(q.Id)).ToList();
        foreach (var q in toDelete) await _testRepo.DeleteQuestionAsync(q.Id);

        // 2. Update / Add
        foreach (var qModel in model.Questions)
        {
            if (qModel.Id.HasValue)
            {
                var q = test.Questions.FirstOrDefault(x => x.Id == qModel.Id.Value);
                if (q != null)
                {
                    q.QuestionText = qModel.QuestionText;
                    q.OptionA = qModel.OptionA;
                    q.OptionB = qModel.OptionB;
                    q.OptionC = qModel.OptionC;
                    q.OptionD = qModel.OptionD;
                    q.CorrectAnswer = qModel.CorrectAnswer;
                    q.Marks = qModel.Marks;
                    q.NegativeMarks = qModel.NegativeMarks;
                    q.Explanation = qModel.Explanation;
                    await _testRepo.UpdateQuestionAsync(q);
                }
            }
            else
            {
                var q = new Question
                {
                    MockTestId = test.Id,
                    QuestionText = qModel.QuestionText,
                    OptionA = qModel.OptionA,
                    OptionB = qModel.OptionB,
                    OptionC = qModel.OptionC,
                    OptionD = qModel.OptionD,
                    CorrectAnswer = qModel.CorrectAnswer,
                    Marks = qModel.Marks,
                    NegativeMarks = qModel.NegativeMarks,
                    Explanation = qModel.Explanation,
                    OrderIndex = model.Questions.IndexOf(qModel) + 1
                };
                await _testRepo.CreateQuestionAsync(q);
            }
        }

        // Updated totals
        var finalQuestions = await _testRepo.GetQuestionsByTestAsync(test.Id);
        test.TotalQuestions = finalQuestions.Count;
        test.TotalMarks = finalQuestions.Sum(q => q.Marks);

        await _testRepo.UpdateTestAsync(test);
        TempData["Success"] = "Test updated successfully!";
        return RedirectToAction("Tests");
    }

    [HttpGet]
    public async Task<IActionResult> ViewTest(int id)
    {
        var test = await _testRepo.GetTestWithQuestionsAsync(id);
        if (test == null) return NotFound();
        return View(test);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTest(int id)
    {
        await _testRepo.DeleteTestAsync(id);
        TempData["Success"] = "Test deleted successfully!";
        return RedirectToAction("Tests");
    }

    // ══════════════════════════════════
    //  EXCEL QUESTION UPLOAD
    // ══════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> UploadQuestionsExcel(int id)
    {
        var test = await _testRepo.GetTestByIdAsync(id);
        if (test == null) return NotFound();

        var model = new ExcelUploadViewModel
        {
            TestId = test.Id,
            TestTitle = test.Title
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadQuestionsExcel(int id, IFormFile? file)
    {
        var test = await _testRepo.GetTestByIdAsync(id);
        if (test == null) return NotFound();

        var model = new ExcelUploadViewModel { TestId = test.Id, TestTitle = test.Title };

        if (file == null || file.Length == 0)
        {
            model.Errors.Add("Please select an Excel file.");
            return View(model);
        }

        var (questions, errors) = await ParseQuestionsFromExcel(file, test.Id);

        if (errors.Any())
        {
            model.Errors.AddRange(errors);
            return View(model);
        }

        if (questions.Count == 0)
        {
            model.Errors.Add("No valid questions found in the file.");
            return View(model);
        }

        // Insert questions
        await _testRepo.AddQuestionsRangeAsync(questions);
        test.TotalQuestions += questions.Count;
        test.TotalMarks += questions.Sum(q => q.Marks);
        await _testRepo.UpdateTestAsync(test);

        model.SuccessCount = questions.Count;
        TempData["Success"] = $"{questions.Count} questions imported successfully!";
        return RedirectToAction("ViewTest", new { id = test.Id });
    }

    private async Task<(List<Question> Questions, List<string> Errors)> ParseQuestionsFromExcel(IFormFile file, int testId)
    {
        var questions = new List<Question>();
        var errors = new List<string>();

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
        {
            errors.Add("Only .xlsx files are supported.");
            return (questions, errors);
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                errors.Add("The Excel file is empty.");
                return (questions, errors);
            }

            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (rowCount < 2)
            {
                errors.Add("No question data found in page (starting from row 2).");
                return (questions, errors);
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var questionText = worksheet.Cell(row, 1).GetString().Trim();
                var optionA = worksheet.Cell(row, 2).GetString().Trim();
                var optionB = worksheet.Cell(row, 3).GetString().Trim();
                var optionC = worksheet.Cell(row, 4).GetString().Trim();
                var optionD = worksheet.Cell(row, 5).GetString().Trim();
                var correctAnswer = worksheet.Cell(row, 6).GetString().Trim().ToUpper();
                var marksStr = worksheet.Cell(row, 7).GetString().Trim();
                var explanation = worksheet.Cell(row, 8).GetString().Trim();

                if (string.IsNullOrEmpty(questionText) && string.IsNullOrEmpty(optionA)) continue;

                if (string.IsNullOrEmpty(questionText)) { errors.Add($"Row {row}: QuestionText missing."); continue; }
                if (string.IsNullOrEmpty(optionA) || string.IsNullOrEmpty(optionB) || 
                    string.IsNullOrEmpty(optionC) || string.IsNullOrEmpty(optionD)) 
                { 
                    errors.Add($"Row {row}: All 4 options are required."); continue; 
                }

                if (!new[] { "A", "B", "C", "D" }.Contains(correctAnswer))
                {
                    errors.Add($"Row {row}: Correct answer must be A, B, C, or D."); continue;
                }

                if (!int.TryParse(marksStr, out int marks)) marks = 1;

                questions.Add(new Question
                {
                    MockTestId = testId,
                    QuestionText = questionText,
                    OptionA = optionA,
                    OptionB = optionB,
                    OptionC = optionC,
                    OptionD = optionD,
                    CorrectAnswer = correctAnswer,
                    Marks = marks,
                    Explanation = explanation,
                    OrderIndex = questions.Count + 1
                });
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error reading Excel file: {ex.Message}");
        }

        return (questions, errors);
    }

    [HttpGet]
    public IActionResult DownloadExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Questions");

        // Header
        var headers = new[] { "QuestionText", "OptionA", "OptionB", "OptionC", "OptionD", "CorrectAnswer", "Marks", "Explanation" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Sample row
        ws.Cell(2, 1).Value = "What is CPU?";
        ws.Cell(2, 2).Value = "Central Processing Unit";
        ws.Cell(2, 3).Value = "Control Processing Unit";
        ws.Cell(2, 4).Value = "Computer Processing Unit";
        ws.Cell(2, 5).Value = "Central Power Unit";
        ws.Cell(2, 6).Value = "A";
        ws.Cell(2, 7).Value = 2;
        ws.Cell(2, 8).Value = "CPU stands for Central Processing Unit, the brain of the computer.";

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "QuestionUploadTemplate.xlsx");
    }

    // ══════════════════════════════════
    //  LECTURES
    // ══════════════════════════════════
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

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var lecture = new Lecture
        {
            Title = model.Title,
            Description = model.Description,
            VideoUrl = model.VideoUrl,
            ThumbnailUrl = model.ThumbnailUrl,
            BranchId = model.BranchId,
            Subject = model.Subject,
            DurationMinutes = model.DurationMinutes,
            CreatedByUserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _lectureRepo.CreateAsync(lecture);
        TempData["Success"] = "Lecture created successfully!";
        return RedirectToAction("Lectures");
    }

    [HttpGet]
    public async Task<IActionResult> EditLecture(int id)
    {
        var lecture = await _lectureRepo.GetByIdAsync(id);
        if (lecture == null) return NotFound();

        var model = new EditLectureViewModel
        {
            Id = lecture.Id,
            Title = lecture.Title,
            Description = lecture.Description,
            VideoUrl = lecture.VideoUrl,
            ThumbnailUrl = lecture.ThumbnailUrl,
            BranchId = lecture.BranchId,
            Subject = lecture.Subject,
            DurationMinutes = lecture.DurationMinutes,
            IsActive = lecture.IsActive,
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLecture(EditLectureViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var lecture = await _lectureRepo.GetByIdAsync(model.Id);
        if (lecture == null) return NotFound();

        lecture.Title = model.Title;
        lecture.Description = model.Description;
        lecture.VideoUrl = model.VideoUrl;
        lecture.ThumbnailUrl = model.ThumbnailUrl;
        lecture.BranchId = model.BranchId;
        lecture.Subject = model.Subject;
        lecture.DurationMinutes = model.DurationMinutes;
        lecture.IsActive = model.IsActive;

        await _lectureRepo.UpdateAsync(lecture);
        TempData["Success"] = "Lecture updated successfully!";
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

    // ══════════════════════════════════
    //  NOTES
    // ══════════════════════════════════
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

    [HttpGet]
    public async Task<IActionResult> EditNotes(int id)
    {
        var notes = await _notesRepo.GetByIdAsync(id);
        if (notes == null) return NotFound();

        var model = new EditNotesViewModel
        {
            Id = notes.Id,
            Title = notes.Title,
            Description = notes.Description,
            BranchId = notes.BranchId,
            Subject = notes.Subject,
            IsActive = notes.IsActive,
            Branches = await _branchRepo.GetAllActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNotes(EditNotesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _branchRepo.GetAllActiveAsync();
            return View(model);
        }

        var notes = await _notesRepo.GetByIdAsync(model.Id);
        if (notes == null) return NotFound();

        notes.Title = model.Title;
        notes.Description = model.Description;
        notes.BranchId = model.BranchId;
        notes.Subject = model.Subject;
        notes.IsActive = model.IsActive;

        if (model.File != null && model.File.Length > 0)
        {
            // Delete old file if exists
            if (!string.IsNullOrEmpty(notes.FileUrl))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, notes.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
            }

            // Save new file
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
            var ext = Path.GetExtension(model.File.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", "Invalid file type. Allowed: PDF, DOC, DOCX, PPT, PPTX");
                model.Branches = await _branchRepo.GetAllActiveAsync();
                return View(model);
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "notes");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            notes.FileUrl = $"/uploads/notes/{fileName}";
            notes.FileSizeBytes = model.File.Length;
            notes.FileType = ext.TrimStart('.').ToUpper();
        }

        await _notesRepo.UpdateAsync(notes);
        TempData["Success"] = "Notes updated successfully!";
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

    // ══════════════════════════════════
    //  ANNOUNCEMENTS
    // ══════════════════════════════════
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
