using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Managment.Models
{
    public class InventoryField
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public FieldType Type { get; set; }

        public string? Slot { get; set; }

        public int Order { get; set; }

        public bool ShowInTable { get; set; } = true;

    }
    
}
