using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
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
                ProfileImageUrl = "/assets/images/defaulticon.jpg"
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
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = appUser.Email }, Request.Scheme);

            var body = $"Please confirm your email by clicking <a href='{confirmationLink}'>this link</a>.";

            try
            {
                await _emailService.SendMailAsync(appUser.Email, "Email Confirmation", body, true);
            }
            catch
            {
                ModelState.AddModelError("", "Unable to send confirmation email. Please try again later.");
                return View(registerVM);
            }

            return RedirectToAction(nameof(SuccessfullyRegistered));
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
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
            if (!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username/email or password is incorrect.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistent, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to multiple failed attempts. Try again later.");
                return View(loginVM);
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                return View(loginVM);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username/email or password is incorrect.");
                return View(loginVM);
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordVM);

            var user = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = System.Net.WebUtility.UrlEncode(token);

            string resetLink = Url.Action("ResetPassword", "Account",
                new { token = tokenEncoded, email = forgotPasswordVM.Email },
                Request.Scheme);

            string body = $"Please click <a href='{resetLink}'>here</a> to reset your password.";
            await _emailService.SendMailAsync(forgotPasswordVM.Email, "Reset your password", body, isHtml: true);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordVM
            {
                Token = token?.Trim(),
                Email = email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {

            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound();

            var decodedToken = Uri.UnescapeDataString(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);

            string emailBody = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            await _emailService.SendMailAsync(user.Email, "Email Confirmation", emailBody, true);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }



        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
