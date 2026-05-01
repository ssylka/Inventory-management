using Inventory_Managment.Models;
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
        public InventoryController(AppDbContext context, UserManager<AppUser> userManager, InventoryService inventoryService)
        {
            _context = context;
            _userManager = userManager;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories.ToListAsync();
            return View(inventories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            ModelState.Remove(nameof(Inventory.CreatorId));//  ВРЕМЕНННО. УБРАТЬ ПРИ СОЗДАНИИ АВТОРИЗАЦИИ !!!!!!!!!!!!!
            inventory.CreatorId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
                return View(inventory);

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        //[ServiceFilter]
        public async Task<IActionResult> Delete([FromBody] List<int> ids) // НЕ ЗАУБДЬ ПРО оптимитсик лок!
        {
            var items = await _context.Inventories
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (items.Count != ids.Count)
                return Conflict("Some inventories no longer exist.\nPlease try again.");

            _context.Inventories.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok();
        }
        //[ServiceFilter]
        public async Task<IActionResult> Fields(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            return View(inventory);
        }
        //[ServiceFilter]
        public IActionResult AddField(int id)
        {
            ViewBag.InventoryId = id;
            return View();
        }
        //[ServiceFilter]

        [HttpPost]
        public async Task<IActionResult> AddField(InventoryField field)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InventoryId = field.InventoryId;
                return View(field);
            }
            try
            {
                field.Id = 0; // Ensure EF Core treats this as a new entity
                field.Slot = _inventoryService.GetNextSlot(field.Type, field.InventoryId);
            }
            catch (Exception ex)
            {
                ViewBag.InventoryId = field.InventoryId;
                TempData["Error"] = ex.Message;
                return View(field);
            }

            _context.InventoryFields.Add(field);
            await _context.SaveChangesAsync();

            return RedirectToAction("Fields", new { id = field.InventoryId });
        }
        
    }
}
