using System.ComponentModel.DataAnnotations;

namespace Inventory_Managment.Models.Dto
{
    public class SalesforceFormViewModel
    {
        public string UserId { get; set; } = "";

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Phone]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Company is required")]
        public string Title { get; set; } = "";
    }
}