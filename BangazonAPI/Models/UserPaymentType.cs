using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class UserPaymentType
    {
        [Required]
        public int id { get; set; }
        [Required]
        [StringLength(55)]
        public string acctNumber { get; set; }
        [Required]
        public bool active { get; set; } = true;
        [Required]
        public int customerId { get; set; }
        [Required]
        public int paymentTypeId { get; set; }
    }
}
