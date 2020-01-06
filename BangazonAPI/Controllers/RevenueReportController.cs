using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueReportController : Controller
    {
        private readonly IConfiguration config;

        public RevenueReportController(IConfiguration _config)
        {
            config = _config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet] 
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT p.Id, pt.Id AS ProductTypeId, p.Price, p.Title, pt.[Name], op.OrderId AS OrderId, o.UserPaymentTypeId AS PaymentType FROM ProductType pt
                                        LEFT JOIN Product p ON pt.id = p.ProductTypeId
                                        LEFT JOIN OrderProduct op ON op.ProductId = p.Id
                                        LEFT JOIN [Order] o ON o.Id = op.OrderId";

                    
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var revenueReport = new RevenueReport();

                    while (reader.Read())
                    {
                        var typeId = reader.GetInt32(reader.GetOrdinal("productTypeId"));
                        var typeAlreadyAdded = revenueReport.ProductTypes.FirstOrDefault(p => p.Id == typeId);
                        
                        if(typeAlreadyAdded == null)
                        {
                            ProductType productType = new ProductType
                            {
                                Id = typeId,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                TotalRevenue = 0
                            };
                            if (!reader.IsDBNull(reader.GetOrdinal("PaymentType")))
                            {
                                productType.TotalRevenue += reader.GetDecimal(reader.GetOrdinal("Price"));
                            }

                            revenueReport.ProductTypes.Add(productType);
                        }
                        else
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("PaymentType")))
                            {
                                typeAlreadyAdded.TotalRevenue += reader.GetDecimal(reader.GetOrdinal("Price"));
                            }
                        }
                    }
                    reader.Close();
                    return Ok(revenueReport);
                }
            }
        }
    }

}
