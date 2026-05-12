using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory_Managment.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ItemService _itemService;

        public ItemController(AppDbContext context, ItemService itemService)
        {
            _context = context;
            _itemService = itemService;
        }
        [AllowAnonymous] // All items are viewable by everyone, regardless of authentication status.
        public async Task<IActionResult> Index(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);
                
            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

            items = items.OrderBy(i => i.CustomId).ToList();

            ViewBag.Inventory = inventory;

            return View(items);
        }

        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Create(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            ViewBag.Inventory = inventory;
            ViewBag.InventoryId = inventoryId;

            return View(new Item{ InventoryId = inventoryId });
        }

        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Create(Item item)
        {
            if (!ModelState.IsValid)
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Fields)
                    .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

                ViewBag.Inventory = inventory;
                ViewBag.InventoryId = item.InventoryId;

                return View(item);
            }

            // Generate only on first attempt; on retry the user may have edited the value
            if (string.IsNullOrEmpty(item.CustomId))
                item.CustomId = await _itemService.GenerateCustomIdAsync(item.InventoryId);

            item.CreatedAt = DateTime.UtcNow;
            _context.Items.Add(item);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Fields)
                    .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

                ViewBag.Inventory = inventory;
                ViewBag.InventoryId = item.InventoryId;
                ModelState.AddModelError("CustomId", "This ID already exists. Please edit the value below and try again.");
                return View(item);
            }

            return Redirect($"/Inventory/Details/{item.InventoryId}#items");
        }
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound();

            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

            ViewBag.Inventory = inventory;

            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Edit(Item item)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

            ViewBag.Inventory = inventory;
            ViewBag.InventoryId = item.InventoryId;

            if (!ModelState.IsValid)
            {
                return View(item);
            }
            try
            {
                _context.Items.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "This item has been or is being modified by another user. Please return to the item list.";

                return View(item);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "There is an error with CustomID. Please, set a valid and unique CustomID.";

                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(item);
            }
            return Redirect($"/Inventory/Details/{item.InventoryId}#items");
        }
        [HttpPost]
        [Authorize(Roles = "Active,Admin")]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var items = await _context.Items
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (items.Count != ids.Count)
            {
                return BadRequest("Some items no longer exist. Please try again.");
            }
            try
            {
                _context.Items.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Some items no longer exist. Please try again.");
            }

            return Ok();
        }
    }

}
