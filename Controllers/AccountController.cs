using Inventory_Managment.Models;
using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly SalesforceService _salesforceService;


        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext context,
            EmailService emailService,
            SalesforceService salesforceService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _salesforceService = salesforceService;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string name)
        {
            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("email", "Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "Password is required.");

            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("name", "Name is required.");

            if (!ModelState.IsValid)
                return View();

            name = name.Trim();

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

            return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");

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
            return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                     ?? info.Principal.FindFirstValue("urn:github:email")
                     ?? info.Principal.FindFirstValue("email");

            // GitHub может не вернуть email (приватный аккаунт без публичного адреса)
            if (string.IsNullOrEmpty(email))
            {
                var login = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? info.Principal.FindFirstValue("urn:github:login");
                if (string.IsNullOrEmpty(login))
                {
                    TempData["Error"] = "Could not retrieve email from your GitHub account. Please make your email public in GitHub settings and try again.";
                    return RedirectToAction("Login");
                }
                email = $"{login}@github.invalid";
            }

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

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAuthenticated = User.Identity!.IsAuthenticated;

            var owned = await _context.Inventories
                .Where(i => i.CreatorId == id)
                .ToListAsync();

            var access = await _context.InventoryAccess
                .Where(a => a.UserId == id)
                .Include(a => a.Inventory)
                .Select(a => a.Inventory!)
                .ToListAsync();

            var roles = (await _userManager.GetRolesAsync(user)).OrderDescending();

            var vm = new ProfileViewModel
            {
                User = user,
                Role = string.Join(", ",roles) ?? "User",
                OwnedInventories = owned,
                AccessInventories = access,
                IsOwnProfile = currentUserId == id
            };

            return View(vm);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SalesforceForm(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id && !isAdmin)
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new SalesforceFormViewModel
            {
                UserId = id,
                Name = user.Name ?? "",
                Email = user.Email ?? ""
            };

            return View(vm);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SalesforceForm(SalesforceFormViewModel vm)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != vm.UserId && !isAdmin)
                return Forbid();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _salesforceService.CreateAccountAndContactAsync(
                    vm.Name, vm.Email, vm.Phone, vm.Title);

                TempData["SalesforceSuccess"] = "Successfully added to Salesforce!";
                return RedirectToAction("Profile", new { id = vm.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Salesforce error: {ex.Message}");
                return View(vm);
            }
        }
    }
}
