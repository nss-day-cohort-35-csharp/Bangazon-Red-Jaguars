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
                    cmd.CommandText = @" SELECT pt.Id AS ProductTypeId, ISNULL(SUM(sales.Price),0) AS Price, pt.[Name] FROM ProductType pt
                                        LEFT JOIN 
                                            (
                                            SELECT p.Price, p.ProductTypeId FROM Product p 
                                            JOIN OrderProduct op ON op.ProductId = p.Id
                                            JOIN [Order] o ON o.Id = op.OrderId
                                            WHERE o.UserPaymentTypeId is not null
                                            ) 
                                            Sales ON sales.ProductTypeId = pt.Id
                                        GROUP BY pt.Id, pt.[Name]";

                    
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var revenueReport = new RevenueReport();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            TotalRevenue = reader.GetDecimal(reader.GetOrdinal("Price"))
                        };
                        
                        revenueReport.ProductTypes.Add(productType);
                    }
                    reader.Close();
                    return Ok(revenueReport);
                }
            }
        }
    }

}
