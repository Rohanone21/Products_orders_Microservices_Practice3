using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orders_API_Microservices.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        public bool IsPaid { get; set; }

    }
}
