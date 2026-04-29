using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Filters
{
    public class InventoryEditFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public InventoryEditFilter(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.ContainsKey("id"))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var inventoryId = (int)context.ActionArguments["id"];

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            var user = context.HttpContext.User;
            var userId = _userManager.GetUserId(user);

            var isAdmin = user.IsInRole("Admin");

            if (inventory.CreatorId != userId && !isAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
