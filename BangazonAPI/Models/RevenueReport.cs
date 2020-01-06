using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class RevenueReport
    {
        public List<ProductType> ProductTypes { get; set; } = new List<ProductType>();
    }
}
