using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypesController : ControllerBase

    //pattern of read only field, typically of an interface. Dependancy injection.
    {
        private readonly IConfiguration _config;

        //the constructor, setting the field
        public PaymentTypesController(IConfiguration config)
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
                    cmd.CommandText = "SELECT Id, Name, Active FROM PaymentType WHERE Active = 1";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Payment> payments = new List<Payment>();

                    while (reader.Read())
                    {
                        Payment payment = new Payment
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };

                        payments.Add(payment);
                    }
                    reader.Close();

                    //Ok method is inheritated by the Controller method. The Ok method returns/wraps it up in a HTTP response with a 200 response code.
                    return Ok(payments);
                }
            }
        }

        [HttpGet("{id}", Name = "GetPayment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, Active
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Payment payment = null;

                    if (reader.Read())
                    {
                        payment = new Payment
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };
                    }
                    reader.Close();

                    if (payment == null)
                    {
                        return NotFound($"No payment found with ID of {id}");
                    };
                    
                    return Ok(payment);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Payment payment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType (Name, Active)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @active)";
                    cmd.Parameters.Add(new SqlParameter("@name", payment.Name));
                    cmd.Parameters.Add(new SqlParameter("@active", payment.Active));

                    int newId = (int)cmd.ExecuteScalar();
                    payment.Id = newId;
                    return CreatedAtRoute("GetPayment", new { id = newId }, payment);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Payment payment)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                            SET Name = @name,
                                                Active = @active
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", payment.Name));
                        cmd.Parameters.Add(new SqlParameter("@active", payment.Active));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
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
                if (!PaymentExist(id))
                {
                    return NotFound();
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
                        cmd.CommandText = @"UPDATE PaymentType
                                          Set Active = 0
                                          WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));


                        int rowsAffected = cmd.ExecuteNonQuery();
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

                if (!PaymentExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Active
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}