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
    public class CustomersController: Controller
    {
        private readonly IConfiguration config;

        public CustomersController(IConfiguration _config)
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

        [HttpGet] ///Get all Custer with optional customerID query
        public async Task<IActionResult> Customers([FromQuery]string? q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Customer.Id, Customer.FirstName, Customer.LastName, Customer.CreatedDate, Customer.Active, Customer.Address, Customer.City, Customer.State, Customer.Email, Customer.Phone " +
                    "FROM Customer ";
                    if (!String.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += "WHERE FirstName LIKE @query or LastName LIKE @query or Address LIKE @query or City LIKE @query or State LIKE @query Email LIKE @query Phone LIKE @query";
                        cmd.Parameters.Add(new SqlParameter("@query", "%" + q + "%"));
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Order> orders = new List<Order>();
                }
            }
        }
    }
}