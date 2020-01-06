using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : Controller
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

        [HttpGet] ///Get all Customer with optional customerID query
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
                        cmd.CommandText += "WHERE Active = 1 and (FirstName LIKE @query or LastName LIKE @query or Address LIKE @query or City LIKE @query or State LIKE @query or Email LIKE @query or Phone LIKE @query)";
                        cmd.Parameters.Add(new SqlParameter("@query", "%" + q + "%"));
                    }

                    else{
                        cmd.CommandText += "WHERE Active = 1";
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        int customerId = reader.GetInt32(reader.GetOrdinal("Id"));
                        Customer customer = new Customer
                        {
                            id = customerId,
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName")),
                            createdDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            address = reader.GetString(reader.GetOrdinal("Address")),
                            city = reader.GetString(reader.GetOrdinal("City")),
                            state = reader.GetString(reader.GetOrdinal("State")),
                            email = reader.GetString(reader.GetOrdinal("Email")),
                            phone = reader.GetString(reader.GetOrdinal("Phone")),
                        };
                        customers.Add(customer);
                    }
                    reader.Close();

                    if (customers.Count == 0)
                    {
                        return NotFound("No results were found");
                    }
                    else
                    {
                        return Ok(customers);
                    }
                }
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Customers([FromRoute]int id, [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Customer.Id, Customer.FirstName, Customer.LastName, Customer.CreatedDate, Customer.Active, Customer.Address, Customer.City, Customer.State, Customer.Email, Customer.Phone";
                    if (include == "products")
                    {
                        cmd.CommandText += ", Product.Id as ProductID, Product.DateAdded as ProductDateAdded, Product.ProductTypeId as ProductTypeId, Product.CustomerId as ProductCustomerId, Product.Price as ProductPrice, Product.Title as ProductTitle, Product.Description as ProductDescription " +
                        "FROM Customer " +
                        "LEFT JOIN Product ON Product.CustomerId = Customer.Id " +
                        "WHERE Customer.Id = @id and Active = 1";
                    }
                    else
                    {
                        cmd.CommandText += " FROM Customer " +
                        "WHERE Customer.Id = @id and Active = 1";
                    }

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    Customer wantedCustomer = null;
                    while (reader.Read())
                    {
                        if (wantedCustomer == null)
                        {
                            wantedCustomer = new Customer
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                lastName = reader.GetString(reader.GetOrdinal("LastName")),
                                createdDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                address = reader.GetString(reader.GetOrdinal("Address")),
                                city = reader.GetString(reader.GetOrdinal("City")),
                                state = reader.GetString(reader.GetOrdinal("State")),
                                email = reader.GetString(reader.GetOrdinal("Email")),
                                phone = reader.GetString(reader.GetOrdinal("Phone")),
                                products = new List<Product>()
                            };
                        }

                        if (include == "products")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                            {
                                int currentProductID = reader.GetInt32(reader.GetOrdinal("ProductId"));

                                if (wantedCustomer.id == reader.GetInt32(reader.GetOrdinal("ProductCustomerId")))
                                {
                                    Product newProduct = new Product
                                    {
                                        Id = currentProductID,
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("ProductCustomerId")),
                                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                        DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                        Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice")),
                                        Title = reader.GetString(reader.GetOrdinal("ProductTitle")),
                                        Description = reader.GetString(reader.GetOrdinal("ProductDescription"))
                                    };

                                    wantedCustomer.products.Add(newProduct);
                                }
                            }
                        }
                    }

                    if (wantedCustomer == null)
                    {
                        return NotFound();
                    }

                    reader.Close();

                    return Ok(wantedCustomer);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @CreatedDate, @Active, @Address, @City, @State, @Email, @Phone)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", customer.firstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.lastName));
                    cmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@Active", customer.active));
                    cmd.Parameters.Add(new SqlParameter("@Address", customer.address));
                    cmd.Parameters.Add(new SqlParameter("@City", customer.city));
                    cmd.Parameters.Add(new SqlParameter("@State", customer.state));
                    cmd.Parameters.Add(new SqlParameter("@Email", customer.email));
                    cmd.Parameters.Add(new SqlParameter("@Phone", customer.phone));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    customer.id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer updatedCustomer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                            SET FirstName = @FirstName, LastName = @LastName, CreatedDate = @CreatedDate, Active = @Active, Address = @Address, City = @City, State = @State, Email = @Email, Phone = @Phone
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", updatedCustomer.firstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", updatedCustomer.lastName));
                        cmd.Parameters.Add(new SqlParameter("@CreatedDate", updatedCustomer.createdDate));
                        cmd.Parameters.Add(new SqlParameter("@Active", updatedCustomer.active));
                        cmd.Parameters.Add(new SqlParameter("@Address", updatedCustomer.address));
                        cmd.Parameters.Add(new SqlParameter("@City", updatedCustomer.city));
                        cmd.Parameters.Add(new SqlParameter("@State", updatedCustomer.state));
                        cmd.Parameters.Add(new SqlParameter("@Email", updatedCustomer.email));
                        cmd.Parameters.Add(new SqlParameter("@Phone", updatedCustomer.phone));
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
                if (!CustomerExists(id))
                {
                    return NotFound("No ID exists of that type");
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                            SET Active = @Active
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Active", false));
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
                if (!CustomerExists(id))
                {
                    return NotFound("No ID exists of that type");
                }
                else
                {
                    throw;
                }
            }
        }

        ///Helper bool
        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id
                        FROM Customer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}