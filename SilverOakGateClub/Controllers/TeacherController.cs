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

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ITeacherRepository _teacherRepo;
    private readonly INotesRepository _notesRepo;
    private readonly ILectureRepository _lectureRepo;
    private readonly ITestRepository _testRepo;
    private readonly IBranchRepository _branchRepo;
    private readonly IWebHostEnvironment _env;

    public TeacherController(
        ITeacherRepository teacherRepo,
        INotesRepository notesRepo,
        ILectureRepository lectureRepo,
        ITestRepository testRepo,
        IBranchRepository branchRepo,
        IWebHostEnvironment env)
    {
        _teacherRepo = teacherRepo;
        _notesRepo = notesRepo;
        _lectureRepo = lectureRepo;
        _testRepo = testRepo;
        _branchRepo = branchRepo;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var departments = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
        var notes = await _notesRepo.GetByUploaderAsync(userId);
        var lectures = await _lectureRepo.GetByCreatorAsync(userId);
        var tests = await _testRepo.GetByCreatorAsync(userId);

        var model = new TeacherDashboardViewModel
        {
            TeacherName = User.Identity?.Name ?? "",
            AssignedDepartments = departments,
            TotalNotesUploaded = notes.Count,
            TotalLecturesCreated = lectures.Count,
            TotalTestsCreated = tests.Count
        };
        return View(model);
    }

    // ── My Content ──
    public async Task<IActionResult> MyContent()
    {
        var userId = GetUserId();
        var model = new TeacherContentViewModel
        {
            Notes = await _notesRepo.GetByUploaderAsync(userId),
            Lectures = await _lectureRepo.GetByCreatorAsync(userId),
            Tests = await _testRepo.GetByCreatorAsync(userId)
        };
        return View(model);
    }

    // ── Create Notes ──
    [HttpGet]
    public async Task<IActionResult> CreateNotes()
    {
        var userId = GetUserId();
        var model = new CreateNotesViewModel
        {
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNotes(CreateNotesViewModel model, IFormFile? file)
    {
        var userId = GetUserId();

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

        // Backend validate department assignment
        if (model.BranchId.HasValue && !await _teacherRepo.IsTeacherAssignedToDepartmentAsync(userId, model.BranchId.Value))
        {
            TempData["Error"] = "You are not assigned to the selected department.";
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

        string fileUrl = "";
        long fileSize = 0;
        string fileType = "PDF";

        if (file != null && file.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", "Invalid file type. Allowed: PDF, DOC, DOCX, PPT, PPTX");
                model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
                return View(model);
            }

            if (file.Length > 50 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size must be less than 50MB.");
                model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
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
        return RedirectToAction("MyContent");
    }

    // ── Create Lecture ──
    [HttpGet]
    public async Task<IActionResult> CreateLecture()
    {
        var userId = GetUserId();
        var model = new CreateLectureViewModel
        {
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLecture(CreateLectureViewModel model)
    {
        var userId = GetUserId();

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

        if (model.BranchId.HasValue && !await _teacherRepo.IsTeacherAssignedToDepartmentAsync(userId, model.BranchId.Value))
        {
            TempData["Error"] = "You are not assigned to the selected department.";
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
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
            CreatedByUserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _lectureRepo.CreateAsync(lecture);
        TempData["Success"] = "Lecture created successfully!";
        return RedirectToAction("MyContent");
    }

    // ── Create Test ──
    [HttpGet]
    public async Task<IActionResult> CreateTest()
    {
        var userId = GetUserId();
        var model = new CreateTestViewModel
        {
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTest(CreateTestViewModel model)
    {
        var userId = GetUserId();

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

        if (model.BranchId.HasValue && !await _teacherRepo.IsTeacherAssignedToDepartmentAsync(userId, model.BranchId.Value))
        {
            TempData["Error"] = "You are not assigned to the selected department.";
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
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
                model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
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
        return RedirectToAction("MyContent");
    }

    // ── Edit Notes ──
    [HttpGet]
    public async Task<IActionResult> EditNotes(int id)
    {
        var userId = GetUserId();
        var notes = await _notesRepo.GetByIdAsync(id);
        if (notes == null || notes.UploadedByUserId != userId)
            return RedirectToAction("MyContent");

        var model = new EditNotesViewModel
        {
            Id = notes.Id,
            Title = notes.Title,
            Description = notes.Description,
            BranchId = notes.BranchId,
            Subject = notes.Subject,
            IsActive = notes.IsActive,
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNotes(EditNotesViewModel model)
    {
        var userId = GetUserId();
        var notes = await _notesRepo.GetByIdAsync(model.Id);
        if (notes == null || notes.UploadedByUserId != userId)
            return RedirectToAction("MyContent");

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

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
                model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
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
        return RedirectToAction("MyContent");
    }

    // ── Edit Lecture ──
    [HttpGet]
    public async Task<IActionResult> EditLecture(int id)
    {
        var userId = GetUserId();
        var lecture = await _lectureRepo.GetByIdAsync(id);
        if (lecture == null || lecture.CreatedByUserId != userId)
            return RedirectToAction("MyContent");

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
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLecture(EditLectureViewModel model)
    {
        var userId = GetUserId();
        var lecture = await _lectureRepo.GetByIdAsync(model.Id);
        if (lecture == null || lecture.CreatedByUserId != userId)
            return RedirectToAction("MyContent");

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

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
        return RedirectToAction("MyContent");
    }

    [HttpGet]
    public async Task<IActionResult> EditTest(int id)
    {
        var userId = GetUserId();
        var test = await _testRepo.GetTestWithQuestionsAsync(id);
        if (test == null || test.CreatedByUserId != userId)
            return RedirectToAction("MyContent");

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
            Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId),
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
        var userId = GetUserId();
        var test = await _testRepo.GetTestWithQuestionsAsync(model.Id);
        if (test == null || test.CreatedByUserId != userId)
            return RedirectToAction("MyContent");

        if (!ModelState.IsValid)
        {
            model.Branches = await _teacherRepo.GetAssignedDepartmentsAsync(userId);
            return View(model);
        }

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
        return RedirectToAction("MyContent");
    }

    [HttpGet]
    public IActionResult DownloadExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Questions");

        var headers = new[] { "QuestionText", "OptionA", "OptionB", "OptionC", "OptionD", "CorrectAnswer", "Marks", "Explanation" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Sample row
        ws.Cell(2, 1).Value = "What is the capital of France?";
        ws.Cell(2, 2).Value = "Berlin";
        ws.Cell(2, 3).Value = "Madrid";
        ws.Cell(2, 4).Value = "Paris";
        ws.Cell(2, 5).Value = "Rome";
        ws.Cell(2, 6).Value = "C";
        ws.Cell(2, 7).Value = 2;
        ws.Cell(2, 8).Value = "Paris is the capital and most populous city of France.";

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "QuestionTemplate.xlsx");
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
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
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
            errors.Add($"Excel Error: {ex.Message}");
        }
        return (questions, errors);
    }
    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.Parse(claim ?? "0");
    }
}
