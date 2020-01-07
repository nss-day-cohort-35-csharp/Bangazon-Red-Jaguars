using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Customer
    {
        [Required (ErrorMessage = "Error: An id is required.")]
        public int id { get; set; }

        [Required (ErrorMessage = "Error: A first name is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "First Name should be between 1 and 55 characters")]
        public string firstName { get; set; }

        [Required (ErrorMessage = "Error: A last name is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "Last Name should be between 1 and 55 characters")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Error: A date is required.")]
        public DateTime createdDate { get; set; }

        [Required (ErrorMessage = "Error: A boolean is required.")]
        public bool active { get; set; } = true;

        [Required(ErrorMessage = "Error: An address is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "Address should be between 1 and 55 characters")]
        public string address { get; set; }

        [Required(ErrorMessage = "Error: A city is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "City should be between 1 and 55 characters")]
        public string city { get; set; }

        [Required(ErrorMessage = "Error: A state is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "State should be between 1 and 55 characters")]
        public string state { get; set; }

        [Required(ErrorMessage = "Error: An email is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "Email should be between 1 and 55 characters")]
        public string email { get; set; }

        [Required(ErrorMessage = "Error: A phone number is required.")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "Phone Number should be between 1 and 55 characters")]
        public string phone { get; set; }

        public List<Product> products { get; set; }
    }
}