using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class CustomerProduct
    {
        [Required(ErrorMessage = "Error: A customer id is required.")]
        public int customerId { get; set; }
        [Required(ErrorMessage = "Error: A product id is required.")]
        public int productId { get; set; }
    }
}
