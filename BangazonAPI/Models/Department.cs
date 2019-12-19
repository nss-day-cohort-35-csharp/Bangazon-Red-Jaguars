using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BangazonAPI.Models
{
    public class Department
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(55, MinimumLength = 5, ErrorMessage = "Department names requires 5-55 characters")]
        public string Name { get; set; }

        [Required]
        public int Budget { get; set; }

        public string Employee { get; set; }

    }
}
