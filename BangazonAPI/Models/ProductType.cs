using System.ComponentModel.DataAnnotations;


namespace BangazonAPI.Models
{
    public class ProductType
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(55, MinimumLength = 5, ErrorMessage = "Product Type names requires 5-55 characters")]
        public string Name { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
