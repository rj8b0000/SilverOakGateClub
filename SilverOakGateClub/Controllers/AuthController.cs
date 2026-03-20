using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverOakGateClub.Repository;
using SilverOakGateClub.ViewModel;

namespace SilverOakGateClub.Controllers;

public class AuthController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly IBranchRepository _branchRepo;

    public AuthController(IUserRepository userRepo, IBranchRepository branchRepo)
    {
        _userRepo = userRepo;
        _branchRepo = branchRepo;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("SelectBranch");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepo.GetByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Verify password
        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password)));
        if (hash != user.PasswordHash)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
            claims.Add(new Claim("BranchName", user.Branch?.Name ?? ""));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            });

        await _userRepo.UpdateLastLoginAsync(user.Id);

        // Admin goes to admin dashboard directly
        if (user.Role == "Admin")
            return RedirectToAction("Index", "Admin");

        // Student selects branch if not set
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("SelectBranch");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SelectBranch()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Admin");

        var branches = await _branchRepo.GetAllActiveAsync();
        var model = new BranchSelectionViewModel
        {
            Branches = branches,
            SelectedBranchId = GetBranchId()
        };
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectBranch(int branchId)
    {
        var branch = await _branchRepo.GetByIdAsync(branchId);
        if (branch == null)
            return RedirectToAction("SelectBranch");

        var userId = GetUserId();
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            user.BranchId = branchId;
            await _userRepo.UpdateAsync(user);
        }

        // Re-sign in with updated branch claim
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, user!.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("BranchId", branchId.ToString()),
            new("BranchName", branch.Name),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    private int? GetBranchId()
    {
        var branchClaim = User.FindFirstValue("BranchId");
        return branchClaim != null ? int.Parse(branchClaim) : null;
    }
}
