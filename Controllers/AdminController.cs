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

        public async Task<IActionResult> Index()
        {
            // Single query: join Users → UserRoles → Roles, group by user
            var userRoles = await _context.Users
                .GroupJoin(
                    _context.UserRoles,
                    u => u.Id,
                    ur => ur.UserId,
                    (u, urs) => new { u, urs })
                .SelectMany(
                    x => x.urs.DefaultIfEmpty(),
                    (x, ur) => new { x.u, RoleId = ur == null ? null : ur.RoleId })
                .Join(
                    _context.Roles.Select(r => new { r.Id, r.Name }),
                    x => x.RoleId,
                    r => r.Id,
                    (x, r) => new { x.u, RoleName = r.Name })
                .ToListAsync();

            var dtos = userRoles
                .GroupBy(x => x.u)
                .Select(g => new UserAdminDto
                {
                    Id           = g.Key.Id,
                    Name         = g.Key.Name,
                    Email        = g.Key.Email ?? "",
                    IsAdmin      = g.Any(x => x.RoleName == "Admin"),
                    IsBlocked    = g.Any(x => x.RoleName == "Blocked"),
                    IsUnverified = g.Any(x => x.RoleName == "Unverified")
                })
                .ToList();

            return View(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.RemoveFromRolesAsync(user,
                new[] { "Active", "Admin", "Unverified" });

            if (!await _userManager.IsInRoleAsync(user, "Blocked"))
            {
                await _userManager.AddToRoleAsync(user, "Blocked");
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.RemoveFromRoleAsync(user, "Blocked");
            await _userManager.AddToRoleAsync(user, "Active");

            return Ok();
        }
            
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
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

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.RemoveFromRoleAsync(user, "Admin");

            return Ok();
        }
    }
}
