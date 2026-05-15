using Inventory_Managment.Models;
using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private string? CurrentUserId => _userManager.GetUserId(User);

        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUserId = CurrentUserId;

            var users = await _context.Users.ToListAsync();

            var userRolePairs = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                select new { ur.UserId, RoleName = r.Name }
            ).ToListAsync();

            var rolesByUser = userRolePairs
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToHashSet());

            var dtos = users.Select(u =>
            {
                var roles = rolesByUser.TryGetValue(u.Id, out var r) ? r : new HashSet<string>();
                return new UserAdminDto
                {
                    Id           = u.Id,
                    Name         = u.Name,
                    Email        = u.Email ?? "",
                    IsAdmin      = roles.Contains("Admin"),
                    IsBlocked    = roles.Contains("Blocked"),
                    IsUnverified = roles.Contains("Unverified")
                };
            }).ToList();

            return View(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            if (id == CurrentUserId) return BadRequest("You cannot block yourself.");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            await _userManager.UpdateAsync(user);

            await _userManager.RemoveFromRolesAsync(user,
                new[] { "Active", "Admin", "Unverified" });

            if (!await _userManager.IsInRoleAsync(user, "Blocked"))
                await _userManager.AddToRoleAsync(user, "Blocked");

            await _userManager.UpdateSecurityStampAsync(user);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            await _userManager.RemoveFromRoleAsync(user, "Blocked");
            await _userManager.AddToRoleAsync(user, "Active");

            await _userManager.UpdateSecurityStampAsync(user);
            return Ok();
        }
            
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == CurrentUserId) return BadRequest("You cannot delete yourself.");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.AddToRoleAsync(user, "Admin");

            await _userManager.UpdateSecurityStampAsync(user);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.RemoveFromRoleAsync(user, "Admin");

            await _userManager.UpdateSecurityStampAsync(user);
            return Ok();
        }
    }
}
