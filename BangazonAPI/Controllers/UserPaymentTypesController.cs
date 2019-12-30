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
    public class UserPaymentTypesController : ControllerBase

    //pattern of read only field, typically of an interface. Dependancy injection.
    {
        private readonly IConfiguration _config;

        //the constructor, setting the field
        public UserPaymentTypesController(IConfiguration config)
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
                    cmd.CommandText = "SELECT Id, AcctNumber, Active, CustomerId, PaymentTypeId FROM UserPaymentType WHERE Active = 1";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<UserPaymentType> userPaymentTypes = new List<UserPaymentType>();

                    while (reader.Read())
                    {
                        UserPaymentType userPaymentType = new UserPaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),                            
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };

                        userPaymentTypes.Add(userPaymentType);
                    }
                    reader.Close();

                    //Ok method is inheritated by the Controller method. The Ok method returns/wraps it up in a HTTP response with a 200 response code.
                    return Ok(userPaymentTypes);
                }
            }
        }

        [HttpGet("{id}", Name = "GetUserPayment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, AcctNumber, Active, CustomerId, PaymentTypeId
                        FROM UserPaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    UserPaymentType userPaymentType = null;

                    if (reader.Read())
                    {
                        userPaymentType = new UserPaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctName")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };
                    }
                    reader.Close();

                    if (userPaymentType == null)
                    {
                        return NotFound($"No user payment type found with ID of {id}");
                    };

                    return Ok(userPaymentType);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPaymentType userPaymentType)
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