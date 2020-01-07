using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class UserPaymentType
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PaymentTypeId { get; set; }

        [Required]
        [StringLength(55)]
        public string AcctNumber { get; set; }

        [Required]
        public bool Active { get; set; } = true;
    }
}
