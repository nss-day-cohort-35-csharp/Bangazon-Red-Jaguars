using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase

    //pattern of read only field, typically of an interface. Dependancy injection.
    {
        private readonly IConfiguration _config;

        //the constructor, setting the field
        public ProductTypeController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
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
                    cmd.CommandText = "SELECT Id, Name FROM ProductType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> productTypes = new List<ProductType>();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };

                        productTypes.Add(productType);
                    }
                    reader.Close();

                    //Ok method is inheritated by the Controller method. The Ok method returns/wraps it up in a HTTP response with a 200 response code.
                    return Ok(productTypes);
                }
            }
        }

        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "products")
                    {
                        cmd.CommandText = @"
                      SELECT pt.[Name] AS ProductTypeName, p.DateAdded, p.ProductTypeId AS ProductTypeId, p.CustomerId, p.Price, p.Title, p.[Description]
                      FROM ProductType pt
                      LEFT JOIN Product p
                      ON pt.Id= p.ProductTypeId
                      WHERE pt.Id = @id";
                    }
                    else
                    {
                        cmd.CommandText = @"
                      SELECT [Name] AS ProductTypeName, Id AS ProductTypeId
                      FROM ProductType 
                      WHERE Id = @id";
                    }
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    ProductType productType = new ProductType();

                    while (reader.Read())
                    {
                        productType.Id = reader.GetInt32(reader.GetOrdinal("ProductTypeId"));
                        productType.Name = reader.GetString(reader.GetOrdinal("ProductTypeName"));

                        if(include == "products")
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description"))
                            };

                            productType.Products.Add(product);
                        }
                    }

                    reader.Close();
                    return Ok(productType);
                }
            }
        }
        

        

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", productType.Name));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    productType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { id = newId }, productType);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType productType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name = @name
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", productType.Name));
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
                if (!ProductTypeExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}