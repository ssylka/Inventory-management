using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);
                
            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

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

            return RedirectToAction("Index", new { inventoryId = item.InventoryId });
        }
    }
}
