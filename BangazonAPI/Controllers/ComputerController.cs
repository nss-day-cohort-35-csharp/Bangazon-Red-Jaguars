using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] bool? available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model FROM Computer c";

                    if (available == true)
                    {
                        cmd.CommandText += @" LEFT JOIN Employee e ON e.ComputerId = c.Id
                                              WHERE e.Id IS NULL AND c.DecomissionDate IS NULL";
                        
                    }

                    if (available == false)
                    {
                        cmd.CommandText += @" LEFT JOIN Employee e ON e.ComputerId = c.Id
                                              WHERE e.Id IS NOT NULL OR c.DecomissionDate IS NOT NULL";

                    }

                    



                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Computer> computers = new List<Computer>();


                    while (reader.Read())
                    {

                        

                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.GetNullableDateTime("DecomissionDate"),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                        };
                        computers.Add(computer);
                    }
                    reader.Close();
                    //from controllerbase interface - returns official json result with 200 status code
                    return Ok(computers);
                }
            }
        }


        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Model FROM Computer 
                                       
                                       
                                       
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.GetNullableDateTime("DecomissionDate"),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                        };
                    }
                    reader.Close();

                    if (computer == null)
                    {
                        return NotFound($"No Computer found with the ID of {id}");
                    }
                    return Ok(computer);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, Make, Model)
                                                OUTPUT INSERTED.Id
                                                VALUES (@PurchaseDate, @Make, @Model)";
                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@Model", computer.Model));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    computer.Id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, computer);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                                    SET PurchaseDate = @PurchaseDate,
                                                        DecomissionDate = @DecomissionDate,
                                                        Make = @Make,
                                                        Model = @Model
                                                        
                                                    WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@DecomissionDate", computer.DecomissionDate));
                        cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No Computer with Id of {id}");
                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
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
            if (!IsComputerAssigned(id))
            {
            try
            {
                
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
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
                if (!ComputerExists(id))
                {
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
                else
                {
                    throw;
                }
            }
            }
                else
            {
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }

            private bool ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                SELECT Id, PurchaseDate, DecomissionDate, Make, Model 
                                FROM Computer 
                                WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        private bool IsComputerAssigned(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT FirstName 
                        FROM Employee
                        WHERE ComputerId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();


                    return reader.Read();
                }
            }
        }
    }
}



