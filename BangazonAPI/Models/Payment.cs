using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Payment
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(55, ErrorMessage = "Name character limit is 55. You've exceeded.")]
        public string Name { get; set; }

        [Required]
        public bool Active { get; set; } = true;

    }
}
