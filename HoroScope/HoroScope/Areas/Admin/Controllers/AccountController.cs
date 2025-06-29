using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Utilities.Enums;

namespace HoroScope.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var existingUserByEmail = await _userManager.FindByEmailAsync(registerVM.Email);
            if (existingUserByEmail != null)

            {
                ModelState.AddModelError(nameof(registerVM.Email), "This email is already registered.");
                return View(registerVM);
            }

            var existingUserByUserName = await _userManager.FindByNameAsync(registerVM.UserName);
            if (existingUserByUserName != null)
            {
                ModelState.AddModelError(nameof(registerVM.UserName), "This username is already taken.");
                return View(registerVM);
            }

            AppUser appUser = new AppUser
            {
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                UserName = registerVM.UserName,
                Email = registerVM.Email,
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, UserRole.Member.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, Email = appUser.Email }, Request.Scheme);

            var body = $"Please confirm your email by clicking <a href='{confirmationLink}'>this link</a>.";

            try
            {
                await _emailService.SendMailAsync(appUser.Email, "Email Confirmation", body, true);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to send confirmation email. Please try again later.");
                return View(registerVM);
            }

            return RedirectToAction(nameof(SuccessfullyRegistered), "Account");
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            AppUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest();

            await _signInManager.SignInAsync(user, false);

            return View();
        }

        public IActionResult SuccessfullyRegistered()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl)
        {
            if (!ModelState.IsValid) return View();

            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "username/email or password is incorrect");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistent, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "3 failed attempts, please try again later");
                return View();
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your Email");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "username/email or password is incorrect");
                return View();
            }

            if (returnUrl is null)
            {
                return RedirectToAction("index", "home");
            }

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = role.ToString()
                    });
                }
            }
            return RedirectToAction("index", "home");
        }
    }
}

