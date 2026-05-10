using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Inventory_Managment.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext context,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string name)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Name = name
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Unverified");

            var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var confirmUrl = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token = encodedToken }, Request.Scheme)!;

            var body = $@"
<h2>Welcome!</h2>
<p>Thank you for creating an account.</p>
<p>
To finish setting up your account, please confirm your email address by clicking the button below:
</p>
<p style='margin: 24px 0;'>
    <a href='{confirmUrl}'
       style='
            background-color:#2563eb;
            color:white;
            padding:12px 20px;
            text-decoration:none;
            border-radius:6px;
            display:inline-block;
            font-weight:bold;'>
        Confirm Email
    </a>
</p>
<p>
If the button above does not work, copy and paste this link into your browser:
</p>
<p>
{confirmUrl}
</p>
<p>
If you did not create an account, you can safely ignore this email.
</p>
<hr>
<p style='color:gray;font-size:12px;'>
This email was sent automatically. Please do not reply.
</p>";

            await _emailService.SendEmail(
                email,
                "Confirm your email",
                body);

            ViewBag.EmailSent = true;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, rawToken);

            if (!result.Succeeded)
                return BadRequest("The confirmation link is invalid.");

            await _userManager.RemoveFromRoleAsync(user, "Unverified");
            await _userManager.AddToRoleAsync(user, "Active");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Inventory");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsInRoleAsync(user, "Blocked"))
            {
                ModelState.AddModelError("", "Your account has been blocked.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Inventory");

            if (result.IsNotAllowed)
                ModelState.AddModelError("", "Please confirm your email before logging in.");
            else if (result.IsLockedOut)
                ModelState.AddModelError("", "You are locked out.");
            else
                ModelState.AddModelError("", "Invalid email or password.");

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Inventory");
        }

        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction("Login");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: true);

            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Inventory");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                     ?? info.Principal.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var name = info.Principal.FindFirstValue(ClaimTypes.Name)
                    ?? info.Principal.FindFirstValue("name");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    Name = name ?? email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) return RedirectToAction("Login");
            }
            else
            {
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
            }

            if (!await _userManager.IsInRoleAsync(user, "Active") &&
                !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Unverified");
                await _userManager.AddToRoleAsync(user, "Active");
            }

            await _userManager.AddLoginAsync(user, info); 
            await _signInManager.SignInAsync(user, isPersistent: true);

            return RedirectToAction("Index", "Inventory");
        }

        public async Task<IActionResult> Profile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var inventories = await _context.Inventories
                .Where(i => i.CreatorId == id && (i.IsPublic || User.Identity!.IsAuthenticated))
                .ToListAsync();

            ViewBag.Inventories = inventories;
            return View(user);
        }
    }
}
