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
        public async Task<IActionResult> Orders([FromQuery]int? customerId, bool cart)
        {
            // If the bool cart is true and the customer id isn't null
            if (cart && !String.IsNullOrWhiteSpace(customerId.ToString()))
            {
                //Retrieves order with cart
                Order newOrder = await GetOrderWithCart(customerId);
                // No results are shown if the new order comes null
                if (newOrder == null)
                {
                    return NotFound("No results were shown.");
                }
                else
                {
                    return Ok(newOrder);
                }
            }
            else
            {
                //Else, it gets orders without cart
                List<Order> orders = await GetOrdersWithoutCart(customerId);

                if (orders.Count == 0)
                {
                    return NotFound("No results were found");
                }
                else
                {
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
                         "OrderProduct.OrderId as OPOrderID, OrderProduct.ProductID as OPProductID, " +
                        "P.Id as ProductId, P.DateAdded as ProductDateAdded, P.ProductTypeId as ProductTypeId, P.CustomerId as ProductCustomerID, P.Price, P.Title, P.Description " +
                        "FROM [Order] " +
                        "LEFT JOIN OrderProduct ON [Order].Id = OrderProduct.OrderId " +
                        "LEFT JOIN Product as P ON OrderProduct.ProductId = P.Id " +
                        "WHERE [Order].Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Order wantedOrder = null;
                    while (reader.Read())
                    {
                        //If the order is null
                        if (wantedOrder == null)
                        {
                            //Create a new order
                            wantedOrder = new Order();
                            wantedOrder.id = reader.GetInt32(reader.GetOrdinal("OrderID"));
                            wantedOrder.customerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId"));
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                wantedOrder.userPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            wantedOrder.products = new List<Product>();
                        }

                        //If there is a product id is in the columns
                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            //If the order id matches the product's Order ID
                            if (wantedOrder.id == reader.GetInt32(reader.GetOrdinal("OPOrderId")))
                            {
                                //Make a new product
                                Product newProduct = new Product
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("ProductCustomerId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };
                                // Added product to order
                                wantedOrder.products.Add(newProduct);
                            }
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

        ///GET helper methods
        ///___________________________________________________________________________________________________________________________

        private async Task<List<Order>> GetOrdersWithoutCart(int? customerID)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Order].Id as OrderID, [Order].CustomerId as OrderCustomerID, [Order].UserPaymentTypeId as UserPaymentTypeId, " +
                        "OrderProduct.OrderId as OPOrderID, OrderProduct.ProductID as OPProductID, " +
                        "P.Id as ProductId, P.DateAdded as ProductDateAdded, P.ProductTypeId as ProductTypeId, P.CustomerId as ProductCustomerID, P.Price, P.Title, P.Description " +
                        "FROM [Order] " +
                        "LEFT JOIN OrderProduct ON [Order].Id = OrderProduct.OrderId " +
                        "LEFT JOIN Product as P ON OrderProduct.ProductId = P.Id ";
                    if (!String.IsNullOrWhiteSpace(customerID.ToString()))
                    {
                        cmd.CommandText += "WHERE [Order].CustomerId = @customerId";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerID));
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Order> orders = new List<Order>();
                    while (reader.Read())
                    {
                        // Get current order ID
                        int currentOrderID = reader.GetInt32(reader.GetOrdinal("OrderID"));

                        //Check if the the order is in the list and instantiate it if it exists
                        Order order = orders.FirstOrDefault(o => o.id == currentOrderID);

                        //If it doesn't exist, create a new one and add it to the list
                        if (order == null)
                        {
                            Order newOrder = new Order();

                            newOrder.id = currentOrderID;
                            newOrder.customerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId"));
                            //Check for allowed null values
                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                newOrder.userPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            newOrder.products = new List<Product>();

                            orders.Add(newOrder);
                        }

                        //If there is a product id in the columns
                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            // Search through the orders
                            foreach (Order orderInList in orders)
                            {
                                //If the order id matches the foreign key id
                                if (orderInList.id == reader.GetInt32(reader.GetOrdinal("OPOrderId")))
                                {
                                    //Add the product to the Order's product list
                                    Product newProduct = new Product
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
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
                    }
                    reader.Close();

                    return orders;
                }
            }
        }

        private async Task<Order> GetOrderWithCart(int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Order].Id as OrderID, [Order].CustomerId as OrderCustomerID, [Order].UserPaymentTypeId as UserPaymentTypeId, " +
                        "P.Id as ProductId, P.DateAdded as ProductDateAdded, P.ProductTypeId as ProductTypeId, P.CustomerId as ProductCustomerID, P.Price, P.Title, P.Description " +
                        "FROM [Order] " +
                        "LEFT JOIN OrderProduct ON [Order].Id = OrderProduct.OrderId " +
                        "LEFT JOIN Product as P ON OrderProduct.ProductId = P.Id ";
                    if (!String.IsNullOrWhiteSpace(customerId.ToString()))
                    {
                        cmd.CommandText += "WHERE [Order].CustomerId = @customerId ";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    }
                    cmd.CommandText += "AND [Order].UserPaymentTypeId IS NULL";
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

                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            Product newProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
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

                    reader.Close();

                    return wantedOrder;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerProduct cusProduct)
        {

            //Check to see if shopping cart exists
            Order order = ShoppingCartExists(cusProduct.customerId);

            if (order.id == 0)
            {
                await PostingOrder(order, cusProduct.customerId);
            }

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO OrderProduct (OrderId, ProductId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@OrderId, @ProductId)";

                    cmd.Parameters.Add(new SqlParameter("@OrderId", order.id));
                    cmd.Parameters.Add(new SqlParameter("@ProductId", cusProduct.productId));
                    return CreatedAtRoute("GetOrder", new { order.id }, order);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order updatedOrder)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order]
                                            SET UserPaymentTypeId = @userPayment
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@userPayment", updatedOrder.userPaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(id))
                {
                    return NotFound("No ID exists of that type");
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}/products{productid}")]
        public async Task<IActionResult> Delete([FromRoute] int id, int productid)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE OrderProduct FROM OrderProduct " +
                        "LEFT JOIN [Order] on [Order].Id = OrderProduct.OrderId " +
                        "WHERE OrderProduct.ProductId = @productid AND OrderId = @OrderId AND [Order].UserPaymentTypeId IS NULL";
                        cmd.Parameters.Add(new SqlParameter("@productid", productid));
                        cmd.Parameters.Add(new SqlParameter("@OrderId", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(id))
                {
                    return NotFound("No ID exists of that type.");
                }
                else
                {
                    throw;
                }
            }
        }

        ///Helper bool
        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, CustomerId, UserPaymentTypeId
                        FROM [Order]
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        //Shopping cart exists helper
        private Order ShoppingCartExists(int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, CustomerId, UserPaymentTypeId
                        FROM [Order]
                        WHERE CustomerId = @customerId AND UserPaymentTypeId IS NULL";
                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();
                    Order awaitingOrder = new Order();

                    while (reader.Read())
                    {
                        awaitingOrder.id = reader.GetInt32(reader.GetOrdinal("Id"));
                        awaitingOrder.customerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                    }

                    reader.Close();

                    return awaitingOrder;
                }
            }
        }

        //Create new order
        private async Task<Order> PostingOrder(Order order, int customerId)
        {
            order.customerId = customerId;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@CustomerId)";

                    cmd.Parameters.Add(new SqlParameter("@CustomerId", order.customerId));
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    order.id = newId;
                    return order;
                }
            }
        }
    }
}