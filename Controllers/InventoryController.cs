using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly InventoryService _inventoryService;

        public InventoryController(AppDbContext context, UserManager<AppUser> userManager,
            InventoryService inventoryService)
        {
            _context = context;
            _userManager = userManager;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index(string? tag = null)
        {
            var isAuthenticated = User.Identity!.IsAuthenticated;

            var query = _context.Inventories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(i => i.InventoryTags.Any(it => it.Tag.Name == tag));

            var inventories = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            ViewBag.Tag = tag;
            return View(inventories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields.OrderBy(f => f.Order))
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var isActive = User.IsInRole("Active");

            inventory.TagNames = inventory.InventoryTags
                .Select(it => it.Tag.Name)
                .ToList();

            var accessUsers = await _context.InventoryAccess
                .Where(a => a.InventoryId == id)
                .Include(a => a.User)
                .Select(a => new AccessUserDto { Id = a.User!.Id, Name = a.User.Name, Email = a.User.Email ?? "" })
                .ToListAsync();

            var vm = new InventoryDetailsViewModel
            {
                Inventory = inventory,
                Categories = await _context.Set<Category>().ToListAsync(),
                Items = await _context.Items
                    .Where(i => i.InventoryId == id)
                    .OrderBy(i => i.CustomId)
                    .ToListAsync(),
                CustomIdElements = await _context.CustomIdElements
                    .Where(e => e.InventoryId == id)
                    .OrderBy(e => e.Order)
                    .ToListAsync(),
                CanEditItems = _inventoryService.CanEdit(inventory, userId, isAdmin, isActive),
                CanEditSettings = _inventoryService.CanEditSettings(inventory, userId, isAdmin),
                AccessUsers = accessUsers
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> AutoSave([FromBody] InventoryEditDto dto)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == dto.Id);

            if (inventory == null)
                return NotFound();

            _context.Entry(inventory).Property(x => x.xmin).OriginalValue = dto.xmin;

            inventory.Title = dto.Title;
            inventory.Description = dto.Description;
            inventory.CategoryId = dto.CategoryId;
            inventory.ImageUrl = dto.ImageUrl;
            inventory.IsPublic = dto.IsPublic;
            inventory.TagNames = dto.TagNames;

            try
            {
                await _inventoryService.UpdateTagsForInventoryAsync(inventory);

                var oldElements = _context.CustomIdElements
                    .Where(e => e.InventoryId == dto.Id);
                _context.CustomIdElements.RemoveRange(oldElements);
                _context.CustomIdElements.AddRange(dto.CustomIdElements.Select(e => new CustomIdElement
                {
                    InventoryId = dto.Id,
                    Type = Enum.Parse<CustomIdElementType>(e.Type),
                    Value = string.IsNullOrEmpty(e.Value) ? null : e.Value,
                    Order = e.Order
                }));

                // Access list diff-update: same pattern as tags — remove gone, add new
                var existingAccess = await _context.InventoryAccess
                    .Where(a => a.InventoryId == dto.Id)
                    .ToListAsync();
                var toRemove = existingAccess
                    .Where(a => !dto.AccessUserIds.Contains(a.UserId))
                    .ToList();
                var existingIds = existingAccess.Select(a => a.UserId).ToHashSet();
                var toAdd = dto.AccessUserIds
                    .Where(uid => !existingIds.Contains(uid))
                    .Select(uid => new InventoryAccess { InventoryId = dto.Id, UserId = uid });
                _context.InventoryAccess.RemoveRange(toRemove);
                _context.InventoryAccess.AddRange(toAdd);

                await _context.SaveChangesAsync();

                // Field's order
                if (dto.FieldOrders.Count > 0)
                {
                    var fieldIds = dto.FieldOrders.Select(f => f.Id).ToList();
                    var dbFields = await _context.InventoryFields
                        .Where(f => fieldIds.Contains(f.Id))
                        .ToListAsync();

                    foreach (var fo in dto.FieldOrders)
                    {
                        var dbField = dbFields.FirstOrDefault(f => f.Id == fo.Id);
                        if (dbField != null) dbField.Order = fo.Order;
                    }
                    await _context.SaveChangesAsync();
                }

                await _context.Entry(inventory).ReloadAsync();
                return Ok(new { xmin = inventory.xmin });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Modified by another user. Reload the page.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Set<Category>().ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            ViewBag.Categories = await _context.Set<Category>().ToListAsync();
            inventory.CreatorId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
                return View(inventory);

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return Redirect($"/Inventory/Details/{inventory.Id}#fields");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string term, string? ids)
        {
            // Search by name OR email, exclude already-added users
            var excludeIds = ids?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                             ?? new List<string>();
            var lower = term.ToLower();
            var users = await _context.Users
                .Where(u => !excludeIds.Contains(u.Id)
                         && (u.Name.ToLower().Contains(lower) || (u.Email.ToLower().Contains(lower))))
                .Select(u => new AccessUserDto { Id = u.Id, Name = u.Name, Email = u.Email ?? "" })
                .Take(10)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetTags(string term, string? ids)
        {
            List<string> existingTags = ids?.Split(',').ToList() ?? new List<string>();
            var tags = await _context.Tags
                .Where(t => t.Name.StartsWith(term))
                .Where(t => !existingTags.Contains(t.Id.ToString()))
                .Select(t => t.Name)
                .Take(10)
                .ToListAsync();

            return Ok(tags);
        }
        [Authorize(Roles = "Active,Admin")]
        public IActionResult AddField(int id)
        {
            ViewBag.InventoryId = id;
            return View(new InventoryField { InventoryId = id, ShowInTable = true });
        }
        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> AddField(InventoryField field)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InventoryId = field.InventoryId;
                return View(field);
            }
            try
            {
                field.Id = 0;
                field.Slot = await _inventoryService.GetNextSlotAsync(field.Type, field.InventoryId);
            }
            catch (Exception ex)
            {
                ViewBag.InventoryId = field.InventoryId;
                TempData["Error"] = ex.Message;
                return View(field);
            }

            _context.InventoryFields.Add(field);
            await _context.SaveChangesAsync();

            return Redirect($"/Inventory/Details/{field.InventoryId}#fields");
        }

        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var inventories = await _context.Inventories
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (inventories.Count != ids.Count)
            {
                return BadRequest("Some Inventories no longer exist. Please try again.");
            }
            _context.Inventories.RemoveRange(inventories);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> EditField(int id)
        {
            var field = await _context.InventoryFields
                .FirstOrDefaultAsync(f => f.Id == id);

            if (field == null)
                return NotFound();

            return View(field);
        }
        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> EditField(InventoryField model)
        {
            if (!ModelState.IsValid)
                return View(model);
            try
            {
                _context.InventoryFields.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "This field has been or is being modified by another user. Please return to the inventory's field list.";

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            return Redirect($"/Inventory/Details/{model.InventoryId}#fields");
        }
        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> DeleteFields([FromBody] List<int> ids)
        {
            var inventoryFields = await _context.InventoryFields
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (inventoryFields.Count != ids.Count)
                return BadRequest("Some inventory's fields no longer exist. Please try again.");

            try
            {
                _context.InventoryFields.RemoveRange(inventoryFields);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Some inventory's fields have been modified by another user. Please try again.");
            }
            return Ok();
        }
    }
}
