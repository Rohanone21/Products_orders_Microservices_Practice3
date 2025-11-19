using System.ComponentModel.DataAnnotations;

namespace Orders_MVC_Project.Models.ViewModels
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
