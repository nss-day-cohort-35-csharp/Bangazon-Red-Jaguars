using BangazonAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IConfiguration config;

        public OrdersController(IConfiguration _config)
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

        [HttpGet] ///Get all Orders with optional customerID query
        public async Task<IActionResult> Orders([FromQuery]int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Order].Id as OrderID, [Order].CustomerId as OrderCustomerID, [Order].UserPaymentTypeId as UserPaymentTypeId, " +
                        "P.Id as ProductId, P.DateAdded as ProductDateAdded, P.ProductTypeId as ProductTypeId, P.CustomerId as ProductCustomerID, P.Price, P.Title, P.Description " +
                        "FROM [Order] " +
                        "LEFT JOIN Customer ON [Order].CustomerId = Customer.Id " +
                        "LEFT JOIN Product as P ON Customer.Id = P.CustomerId ";
                    if (!String.IsNullOrWhiteSpace(customerId.ToString()))
                    {
                        cmd.CommandText += "WHERE [Order].CustomerId = @customerId";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Order> orders = new List<Order>();
                    while (reader.Read())
                    {
                        int currentOrderID = reader.GetInt32(reader.GetOrdinal("OrderID"));
                        Order order = orders.FirstOrDefault(o => o.id == currentOrderID);
                        if (order == null)
                        {
                            Order newOrder = new Order();

                            newOrder.id = currentOrderID;
                            newOrder.customerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                newOrder.userPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            newOrder.products = new List<Product>();

                            orders.Add(newOrder);
                        }

                        int currentProductID = reader.GetInt32(reader.GetOrdinal("ProductId"));
                        foreach (Order orderInList in orders)
                        {
                            if (orderInList.customerId == reader.GetInt32(reader.GetOrdinal("ProductCustomerID")) && orderInList.products.FirstOrDefault(p => p.Id == currentProductID) == null)
                            {
                                Product newProduct = new Product
                                {
                                    Id = currentProductID,
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("ProductCustomerId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };

                                orderInList.products.Add(newProduct);
                            }
                        }
                    }
                    reader.Close();

                    return Ok(orders);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Orders([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Order].Id as OrderID, [Order].CustomerId as OrderCustomerID, [Order].UserPaymentTypeId as UserPaymentTypeId, " +
                        "P.Id as ProductId, P.DateAdded as ProductDateAdded, P.ProductTypeId as ProductTypeId, P.CustomerId as ProductCustomerID, P.Price, P.Title, P.Description " +
                        "FROM [Order] " +
                        "LEFT JOIN Customer ON [Order].CustomerId = Customer.Id " +
                        "LEFT JOIN Product as P ON Customer.Id = P.CustomerId " +
                        "WHERE [Order].Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Order wantedOrder = null;
                    while (reader.Read())
                    {
                        if (wantedOrder == null)
                        {
                            wantedOrder = new Order();
                            wantedOrder.id = reader.GetInt32(reader.GetOrdinal("OrderID"));
                            wantedOrder.customerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                wantedOrder.userPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            wantedOrder.products = new List<Product>();
                        }

                        int currentProductID = reader.GetInt32(reader.GetOrdinal("ProductId"));
                     
                        
                            if (wantedOrder.customerId == reader.GetInt32(reader.GetOrdinal("ProductCustomerID")) && wantedOrder.products.FirstOrDefault(p => p.Id == currentProductID) == null)
                            {
                                Product newProduct = new Product
                                {
                                    Id = currentProductID,
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("ProductCustomerId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };

                                wantedOrder.products.Add(newProduct);
                            }
                        
                    }

                    if (wantedOrder == null)
                    {
                        return NotFound();
                    }

                    reader.Close();

                    return Ok(wantedOrder);
                }
            }
        }
    }
}