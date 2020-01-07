using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        [Required]
        public int id { get; set; }
        [Required]
        public int customerId { get; set; }
        public int? userPaymentTypeId { get; set; }
        public List<Product> products { get; set; }
    }
}
