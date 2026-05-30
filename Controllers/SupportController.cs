using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Managment.Models;

namespace Inventory_Managment.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly DropboxService _dropboxService;
        private readonly UserManager<AppUser> _userManager;

        public SupportController(
            DropboxService dropboxService,
            UserManager<AppUser> userManager)
        {
            _dropboxService = dropboxService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateTicket(string? inventoryTitle, string? pageLink)
        {
            var vm = new SupportTicketViewModel
            {
                ReportedBy = _userManager.GetUserName(User) ?? "",
                InventoryTitle = inventoryTitle,
                PageLink = pageLink ?? Request.Headers["Referer"].ToString()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket(SupportTicketViewModel vm)
        {
            vm.ReportedBy = _userManager.GetUserName(User) ?? "";

            var ticket = new
            {
                reported_by = vm.ReportedBy,
                summary = vm.Summary,
                priority = vm.Priority,
                inventory = vm.InventoryTitle ?? "N/A",
                link = vm.PageLink,
                admin_emails = vm.AdminEmails,
                created_at = DateTime.UtcNow.ToString("o")
            };

            try
            {
                await _dropboxService.UploadTicketAsync(ticket);
                TempData["TicketSuccess"] = "Support ticket created successfully!";
                return RedirectToAction("TicketSent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(vm);
            }
        }

        public IActionResult TicketSent() => View();
    }
}