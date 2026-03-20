using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Models;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

[Authorize]
public class NotesController : Controller
{
    private readonly INotesRepository _notesRepo;
    private readonly IWebHostEnvironment _env;

    public NotesController(INotesRepository notesRepo, IWebHostEnvironment env)
    {
        _notesRepo = notesRepo;
        _env = env;
    }

    public async Task<IActionResult> Index(string? subject = null)
    {
        var branchId = GetBranchId();
        if (branchId == null) return RedirectToAction("SelectBranch", "Auth");

        var notes = string.IsNullOrEmpty(subject)
            ? await _notesRepo.GetByBranchAsync(branchId.Value)
            : await _notesRepo.GetBySubjectAsync(branchId.Value, subject);

        var subjects = await _notesRepo.GetSubjectsByBranchAsync(branchId.Value);

        var model = new NotesListViewModel
        {
            BranchName = User.FindFirstValue("BranchName") ?? "",
            BranchId = branchId.Value,
            NotesList = notes,
            Subjects = subjects,
            SelectedSubject = subject
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, double rating)
    {
        if (rating < 1 || rating > 5) return BadRequest();
        await _notesRepo.UpdateRatingAsync(id, rating);
        return Ok();
    }

    public async Task<IActionResult> Download(int id)
    {
        var notes = await _notesRepo.GetByIdAsync(id);
        if (notes == null) return NotFound();

        await _notesRepo.IncrementDownloadCountAsync(id);

        var filePath = Path.Combine(_env.WebRootPath, notes.FileUrl.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        var contentType = notes.FileType?.ToLower() switch
        {
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "doc" => "application/msword",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType, notes.Title + "." + notes.FileType?.ToLower());
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private int? GetBranchId()
    {
        var claim = User.FindFirstValue("BranchId");
        return claim != null ? int.Parse(claim) : null;
    }
}
