using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

[Authorize]
public class LectureController : Controller
{
    private readonly ILectureRepository _lectureRepo;

    public LectureController(ILectureRepository lectureRepo)
    {
        _lectureRepo = lectureRepo;
    }

    public async Task<IActionResult> Index(string? subject = null)
    {
        var branchId = GetBranchId();

        List<Models.Lecture> lectures;
        List<string> subjects;

        if (branchId.HasValue)
        {
            lectures = string.IsNullOrEmpty(subject)
                ? await _lectureRepo.GetByBranchAsync(branchId.Value)
                : await _lectureRepo.GetBySubjectAsync(branchId.Value, subject);
            subjects = await _lectureRepo.GetSubjectsByBranchAsync(branchId.Value);
        }
        else
        {
            lectures = await _lectureRepo.GetAllAsync();
            subjects = new List<string>();
        }

        var model = new LectureListViewModel
        {
            BranchName = User.FindFirstValue("BranchName") ?? "",
            BranchId = branchId ?? 0,
            Lectures = lectures,
            Subjects = subjects,
            SelectedSubject = subject
        };

        return View(model);
    }

    public async Task<IActionResult> Watch(int id)
    {
        var lecture = await _lectureRepo.GetByIdAsync(id);
        if (lecture == null) return NotFound();

        await _lectureRepo.IncrementViewCountAsync(id);

        return View(lecture);
    }

    private int? GetBranchId()
    {
        var claim = User.FindFirstValue("BranchId");
        return claim != null ? int.Parse(claim) : null;
    }
}
