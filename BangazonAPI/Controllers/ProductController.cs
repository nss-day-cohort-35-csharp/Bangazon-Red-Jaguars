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

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery]string firstName, [FromQuery]string lastName, [FromQuery] string slackHandle, [FromQuery] string specialty, [FromQuery] string orderBy, [FromQuery] bool desc)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Specialty, c.Id AS CohortId, c.Name AS CohortName FROM Instructor i
                                       LEFT JOIN Cohort c ON i.CohortId = c.Id

                                        WHERE 1=1";

                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        cmd.CommandText += " AND i.FirstName LIKE @FirstName";
                        cmd.Parameters.Add(new SqlParameter(@"FirstName", firstName));
                    }

                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        cmd.CommandText += " AND i.LastName LIKE @LastName";
                        cmd.Parameters.Add(new SqlParameter(@"LastName", "%" + lastName + "%"));
                    }

                    if (!string.IsNullOrWhiteSpace(slackHandle))
                    {
                        cmd.CommandText += " AND i.SlackHandle LIKE @SlackHandle";
                        cmd.Parameters.Add(new SqlParameter(@"SlackHandle", "%" + slackHandle + "%"));
                    }

                    if (!string.IsNullOrWhiteSpace(specialty))
                    {
                        cmd.CommandText += " AND i.Specialty LIKE @Specialty";
                        cmd.Parameters.Add(new SqlParameter(@"Specialty", "%" + specialty + "%"));
                    }

                    if (orderBy == "FirstName" || orderBy == "LastName" || orderBy == "SlackHandle" || orderBy == "Specialty")
                    {
                        cmd.CommandText += " ORDER BY @column";
                        cmd.Parameters.Add(new SqlParameter(@"column", orderBy));
                    }

                    if (orderBy != null && desc == true)
                    {
                        cmd.CommandText += " DESC";
                    }



                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Instructor> instructors = new List<Instructor>();


                    while (reader.Read())
                    {


                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            InstructorCohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                StudentsInCohort = new List<Student>(),
                                InstructorsInCohort = new List<Instructor>()
                            }

                        };
                        instructors.Add(instructor);
                    }
                    reader.Close();
                    //from controllerbase interface - returns official json result with 200 status code
                    return Ok(instructors);
                }
            }
        }


        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Specialty, c.Id AS CohortId, c.Name AS CohortName FROM Instructor i
                                       LEFT JOIN Cohort c ON i.CohortId = c.Id

                                        WHERE i.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            InstructorCohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                StudentsInCohort = new List<Student>(),
                                InstructorsInCohort = new List<Instructor>()
                            }

                        };
                    }
                    reader.Close();

                    if (instructor == null)
                    {
                        return NotFound($"No Instructor found wit the ID of {id}");
                    }
                    return Ok(instructor);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlackHandle, CohortId, Specialty)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @SlackHandle, @CohortId, @Specialty)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@SlackHandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@CohortId", instructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@Specialty", instructor.Specialty));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    instructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, instructor);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor instructor)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Instructor
                                            SET FirstName = @FirstName,
                                                LastName = @LastName,
                                                SlackHandle = @SlackHandle,
                                                CohortId = @CohortId,
                                                Specialty = @Specialty
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@SlackHandle", instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@CohortId", instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@Specialty", instructor.Specialty));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No instructor with Id of {id}");
                    }
                }
            }
            catch (Exception)
            {
                if (!InstructorExists(id))
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
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";
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
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool InstructorExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, CohortId, Specialty
                        FROM Instructor
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}


