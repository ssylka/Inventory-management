using Inventory_Managment.Attributes;
using Inventory_Managment.Models;
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

        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
                return NotFound();

            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            if (!ModelState.IsValid)
                return View(inventory);
            try
            {
                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "This item has been or is being modified by another user. Please return to the inventory's field list.";

                return View(inventory);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(inventory);
            }
            return RedirectToAction("Index");
            //var existing = await _context.Inventories
            //    .Include(i => i.Fields)
            //    .FirstOrDefaultAsync(i => i.Id == inventory.Id);

            //if (existing == null)
            //    return NotFound();
            //_context.Entry(existing).OriginalValues["xmin"] = inventory.xmin;
            //try
            //{
            //    _context.Entry(existing).CurrentValues.SetValues(inventory);
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    var fresh = await _context.Inventories
            //        .AsNoTracking()
            //        .Include(i => i.Fields)
            //        .FirstOrDefaultAsync(i => i.Id == inventory.Id);

            //    ModelState.Clear();

            //    TempData["Error"] = "This inventory has been or is being modified by another user. Please return to the item list.";
            //    return View(fresh);
            //    //return BadRequest("The inventory has been modified by another user. Please reload the page and try again.");
            //}

            //return RedirectToAction("Index");
        }

        //[ServiceFilter]
        public async Task<IActionResult> Fields(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);
            inventory.Fields = inventory.Fields
                .OrderBy(f => f.Order)
                .ToList();
            return View(inventory);
        }
        //[ServiceFilter]
        public IActionResult AddField(int id)
        {
            ViewBag.InventoryId = id;
            return View(new InventoryField { InventoryId = id, ShowInTable = true });
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
        [HttpPost]
        //[InventoryEdit]
        public async Task<IActionResult> UpdateFieldOrder([FromBody] List<FieldOrderDto> fields)
        {
            foreach (var field in fields)
            {
                var entity = await _context.InventoryFields
                    .FirstOrDefaultAsync(f => f.Id == field.Id);

                if (entity != null)
                {
                    entity.Order = field.Order;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        //[ServiceFilter]
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
        public async Task<IActionResult> EditField(int id)
        {
            var field = await _context.InventoryFields
                .FirstOrDefaultAsync(f => f.Id == id);

            if (field == null)
                return NotFound();

            return View(field);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken] 
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
            return RedirectToAction("Index");
        }
        [HttpPost]
        //[ServiceFilter]
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
