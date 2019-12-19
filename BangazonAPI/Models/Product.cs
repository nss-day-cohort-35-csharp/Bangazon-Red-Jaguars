using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Product
    {
        [Required]
        public int Id { get; set; }
        
        

        

        [Required]
        public int ProductTypeId { get; set; }
        [Required]
        public Decimal Price { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Titles require a minimum of 3 characters and a max of 255")]
        public string Title { get; set; }
       
        [Required]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Descriptions require a minimum of 5 characters and a max of 255")]

        

       

        public string Description { get; set; }

        [Required]

        public int CustomerId { get; set; }
        [Required]

        public DateTime DateAdded { get; set; }

    


    }
}
