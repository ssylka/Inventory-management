using Inventory_Managment.Models;
using Inventory_Managment.Services;
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

        public async Task<IActionResult> Index(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);
                
            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();
            items = items
                .OrderBy(i =>
                    i.CustomId ?? $"ITEM-{i.CreatedAt.Year}-{i.Id:D4}"
                ).ToList();

            ViewBag.Inventory = inventory;

            return View(items);
        }

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
            try
            {
                item.CustomId = await _itemService.GenerateCustomIdAsync(item.InventoryId);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("CustomId", "ID already exists. Try again.");
                return View(item);
            }
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { inventoryId = item.InventoryId });
        }
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

            var items = await _context.Items
                .Where(i => i.InventoryId == inventory.Id)
                .ToListAsync();

            if (item == null)
                return NotFound();

            ViewBag.Inventory = inventory;

            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return View(item);
        }

        [HttpPost]
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
                TempData["Error"] = "This ID already exists.";

                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(item);
            }
            return RedirectToAction("Index", new { inventoryId = item.InventoryId });
        }
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var items = await _context.Items
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (items.Count != ids.Count)
                return Conflict("Some items no longer exist.\nPlease try again.");
            try
            {
                _context.Items.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "This item has been or is being modified by another user. Please return to the item list.";

                return Conflict("Some items no longer exist.\nPlease try again.");
            }

            return Ok();
        }
    }

}
