using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.Utilities.Enums;
using Pronia.Utilities.Extensions;

[Authorize(Roles = "Member")]
public class ProfileController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProfileController(UserManager<AppUser> userManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new UserProfileVM
        {
            Name = user.Name,
            Surname = user.Surname,
            Username = user.UserName,
            BirthDate = user.BirthDate,
            BirthTime = user.BirthTime,
            BirthPlace = user.BirthPlace,
            ProfileImageUrl = user.ProfileImageUrl,
            SunSign = user.SunSign,
            RisingSign = user.RisingSign,
            MoonSign = user.MoonSign,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(UserProfileVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Name = model.Name;
        user.Surname = model.Surname;
        user.BirthDate = model.BirthDate;
        user.BirthTime = model.BirthTime;
        user.BirthPlace = model.BirthPlace;

        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        {
            if (!model.ProfilePhoto.ValidateType("image"))
            {
                ModelState.AddModelError("ProfilePhoto", "Only image files are allowed.");
                return View(model);
            }

            if (!model.ProfilePhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("ProfilePhoto", "Image size must be less than 2MB.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var oldFileName = Path.GetFileName(user.ProfileImageUrl);
                oldFileName.DeleteFile(_env.WebRootPath, "assets", "images");
            }

            var newFileName = await model.ProfilePhoto.CreateFileAsync(_env.WebRootPath, "assets", "images");
            user.ProfileImageUrl = $"/assets/images/{newFileName}";
        }

        user.SunSign = CalculateSunSign(user.BirthDate);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        TempData["Success"] = "Profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    private string CalculateSunSign(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return "Unknown";

        int day = birthDate.Value.Day;
        int month = birthDate.Value.Month;

        if ((month == 3 && day >= 21) || (month == 4 && day <= 19)) return "Aries";
        if ((month == 4 && day >= 20) || (month == 5 && day <= 20)) return "Taurus";
        if ((month == 5 && day >= 21) || (month == 6 && day <= 20)) return "Gemini";
        if ((month == 6 && day >= 21) || (month == 7 && day <= 22)) return "Cancer";
        if ((month == 7 && day >= 23) || (month == 8 && day <= 22)) return "Leo";
        if ((month == 8 && day >= 23) || (month == 9 && day <= 22)) return "Virgo";
        if ((month == 9 && day >= 23) || (month == 10 && day <= 22)) return "Libra";
        if ((month == 10 && day >= 23) || (month == 11 && day <= 21)) return "Scorpio";
        if ((month == 11 && day >= 22) || (month == 12 && day <= 21)) return "Sagittarius";
        if ((month == 12 && day >= 22) || (month == 1 && day <= 19)) return "Capricorn";
        if ((month == 1 && day >= 20) || (month == 2 && day <= 18)) return "Aquarius";
        if ((month == 2 && day >= 19) || (month == 3 && day <= 20)) return "Pisces";

        return "Unknown";
    }
}
