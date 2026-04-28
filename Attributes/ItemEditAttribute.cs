using Inventory_Managment.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Managment.Attributes
{
    public class ItemEditAttribute : TypeFilterAttribute
    {
        public ItemEditAttribute() : base(typeof(ItemEditFilter)) { }
    }
}
