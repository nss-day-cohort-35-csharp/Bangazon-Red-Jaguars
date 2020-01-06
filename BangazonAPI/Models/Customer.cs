using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Customer
    {
        [Required]
        public int id { get; set; }

        [Required]
        [StringLength(55)]
        public string firstName { get; set; }

        [Required]
        [StringLength(55)]
        public string lastName { get; set; }

        [Required]
        public DateTime createdDate { get; set; }

        [Required]
        public bool active { get; set; } = true;

        [Required]
        [StringLength(55)]
        public string address { get; set; }

        [Required]
        [StringLength(55)]
        public string city { get; set; }

        [Required]
        [StringLength(55)]
        public string state { get; set; }

        [Required]
        [StringLength(55)]
        public string email { get; set; }

        [Required]
        [StringLength(55)]
        public string phone { get; set; }

        public List<Product> products { get; set; }
    }
}