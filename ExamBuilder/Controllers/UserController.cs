using ExamBuilder.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExamBuilder.Controllers;

[Authorize(Roles = "Administrator")]
public class UserController : Controller
{
    private readonly UserManager<ExamBuilderUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(UserManager<ExamBuilderUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userRoles = new Dictionary<string, string>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles.FirstOrDefault() ?? "";
        }
        ViewBag.UserRoles = userRoles;
        ViewBag.Rollen = new SelectList(_roleManager.Roles.OrderBy(r => r.Name).ToList(), "Name", "Name");
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string vorname, string nachname, string userName, string email, string rolle, string passwort, string passwortWiederholen)
    {
        if (passwort != passwortWiederholen)
            return Json(new { success = false, message = "Die Passwörter stimmen nicht überein." });

        var user = new ExamBuilderUser
        {
            UserName = userName,
            Email = email,
            Vorname = vorname,
            Nachname = nachname,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, passwort);
        if (!result.Succeeded)
            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        await _userManager.AddToRoleAsync(user, rolle);
        return Json(new { success = true, message = "Benutzer erfolgreich angelegt." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string vorname, string nachname, string userName, string email, string rolle, string? passwort, string? passwortWiederholen)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return Json(new { success = false, message = "Benutzer nicht gefunden." });

        if (!string.IsNullOrEmpty(passwort) && passwort != passwortWiederholen)
            return Json(new { success = false, message = "Die Passwörter stimmen nicht überein." });

        user.Vorname = vorname;
        user.Nachname = nachname;
        user.Email = email;

        if (user.UserName != userName)
            await _userManager.SetUserNameAsync(user, userName);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Json(new { success = false, message = string.Join(" ", updateResult.Errors.Select(e => e.Description)) });

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, rolle);

        if (!string.IsNullOrEmpty(passwort))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwResult = await _userManager.ResetPasswordAsync(user, token, passwort);
            if (!pwResult.Succeeded)
                return Json(new { success = false, message = string.Join(" ", pwResult.Errors.Select(e => e.Description)) });
        }

        return Json(new { success = true, message = "Benutzer erfolgreich aktualisiert." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
            return Json(new { success = false, message = "Sie können sich nicht selbst löschen." });

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return Json(new { success = false, message = "Benutzer nicht gefunden." });

        await _userManager.DeleteAsync(user);
        return Json(new { success = true });
    }
}
