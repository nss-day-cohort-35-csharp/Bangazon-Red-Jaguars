using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class PaymentType
    {
        [Required]
        public int id { get; set; }

        [Required]
        [StringLength(55)]
        public int name { get; set; }

        [Required]
        public bool active { get; set; } = true;
    }
}
