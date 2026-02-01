using MailKit.Net.Smtp;
using MailKit.Security;
using MainBackend.Models;
using MainBackend.Services.Interfaces;
using MainBackend.ViewModels.AccountVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace MainBackend.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IFileService fileService, IEmailService emailService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _emailService = emailService;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM request)
        {
            if (!ModelState.IsValid) return View(request);

            AppUser appUser = new()
            {
                Name = request.Name,
                Surname = request.Surname,
                UserName = request.Username,
                Email = request.Email
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, request.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(request);
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            string confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { userId=appUser.Id, token }, Request.Scheme);

            string emailBody = await _fileService.ReadFile("wwwroot/template/verify.html");

            emailBody = emailBody.Replace("{{link}}", confirmationLink);
            emailBody = emailBody.Replace("{{name}}", appUser.Name);
            emailBody = emailBody.Replace("{{surname}}", appUser.Surname);

            _emailService.SendEmail(appUser.Email, "Confirm Email", emailBody);

            return RedirectToAction(nameof(CheckEmail));
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            AppUser? appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null) return NotFound();

            IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, token);

            if(!result.Succeeded)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult CheckEmail()
        {
            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            ResetPasswordVM resetPasswordVM = new()
            {
                UserId = userId,
                Token = token
            };

            return View(resetPasswordVM);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM request)
        {
            if (!ModelState.IsValid) return View(request);

            AppUser? appUser = await _userManager.FindByIdAsync(request.UserId);

            if (appUser == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(request);
            }

            bool checkPassword = await _userManager.CheckPasswordAsync(appUser, request.NewPassword);

            if (checkPassword)
            {
                ModelState.AddModelError("", "Old Password!");
                return View(request);
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(appUser, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM request)
        {
            if (!ModelState.IsValid) return View(request);

            AppUser appUser = await _userManager.FindByEmailAsync(request.Email);

            if (appUser == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(request);
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            string? resetLink = Url.Action("ResetPassword", "Account",
                new { userId = appUser.Id, token }, Request.Scheme);

            string emailBody = await _fileService.ReadFile("wwwroot/template/verify.html");

            emailBody = emailBody.Replace("{{link}}", resetLink);
            emailBody = emailBody.Replace("{{name}}", appUser.Name);
            emailBody = emailBody.Replace("{{surname}}", appUser.Surname);

            _emailService.SendEmail(appUser.Email, "Reset your password", emailBody);

            return RedirectToAction(nameof(CheckEmail));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM request)
        {
            if (!ModelState.IsValid) return View(request);

            AppUser appUser = await _userManager.FindByNameAsync(request.UsernameOrEmail);

            if (appUser == null)
            {
                appUser = await _userManager.FindByEmailAsync(request.UsernameOrEmail);
            }

            if (appUser == null)
            {
                ModelState.AddModelError("", "Hər hansısa məlumat səhvdir.");
                return View(request);
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, false);

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Hər hansısa məlumat səhvdir.");
                return View();
            }
            else
            {
                await _signInManager.SignInAsync(appUser, false);
            }


            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (var role in Enum.GetValues<Enums.Roles>())
            {
                if (await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    return Json($"{role} already exists!");
                }
                else
                {
                    IdentityRole identityRole = new()
                    {
                        Name = role.ToString()
                    };

                    await _roleManager.CreateAsync(identityRole);
                }
            }

            return Json("Roles were successfully created!");
        }
    }
}
