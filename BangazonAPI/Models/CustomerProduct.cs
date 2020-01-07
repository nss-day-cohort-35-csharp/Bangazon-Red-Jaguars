using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class CustomerProduct
    {
        [Required]
        public int customerId { get; set; }
        [Required]
        public int productId { get; set; }
    }
}
