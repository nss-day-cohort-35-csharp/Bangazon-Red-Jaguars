using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class UserPaymentType
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Account Number is required. Maximum number of 55 characters.")]
        [StringLength(55)]
        public string AcctNumber { get; set; }

        [Required(ErrorMessage = "Active Status of true/false is required.")]
        public bool Active { get; set; } = true;

        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Payment Type ID is required.")]
        public int PaymentTypeId { get; set; }
    }
}
