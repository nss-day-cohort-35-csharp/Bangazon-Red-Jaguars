﻿using System;
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
    public class TrainingProgramsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
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
        //[Route("GetAllTrainingPrograms")]
        public async Task<IActionResult> GetAllTrainingPrograms()
        //public async Task<List<TrainingProgram>> GetAllTrainingPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, [Name], StartDate, EndDate, MaxAttendees FROM TrainingProgram";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    int IdOrdinal = reader.GetOrdinal("Id");
                    int NameOrdinal = reader.GetOrdinal("Name");
                    int StartDateOrdinal = reader.GetOrdinal("StartDate");
                    int EndDateOrdinal = reader.GetOrdinal("EndDate");
                    int MaxAttendeesOrdinal = reader.GetOrdinal("MaxAttendees");

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32( IdOrdinal ),
                            Name = reader.GetString(NameOrdinal),
                            StartDate = reader.GetDateTime(StartDateOrdinal),
                            EndDate = reader.GetDateTime(EndDateOrdinal),
                            MaxAttendees = reader.GetInt32(MaxAttendeesOrdinal)
                        };

                        trainingPrograms.Add( trainingProgram );
                    }
                    reader.Close();

                    return Ok( trainingPrograms );
                }
            }
        }

        [HttpGet("{id}", Name = "GetTrainingProgramById")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], StartDate, EndDate, MaxAttendees 
                        FROM TrainingProgram
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    TrainingProgram trainingProgram = null;

                    if (reader.Read())
                    {
                        trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32( reader.GetOrdinal("Id") ),
                            Name = reader.GetString( reader.GetOrdinal("Name") ),
                            StartDate = reader.GetDateTime( reader.GetOrdinal("StartDate") ),
                            EndDate = reader.GetDateTime( reader.GetOrdinal("EndDate") ),
                            MaxAttendees = reader.GetInt32( reader.GetOrdinal("MaxAttendees") )
                        };
                    }
                    reader.Close();

                    return Ok( trainingProgram );
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.id
                                        VALUES (@name, @startDate, @endDate, @maxAttendees)";

                    cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate ));
                    cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    trainingProgram.Id = newId;

                    return CreatedAtRoute("GetTrainingProgramById", new { id = newId }, trainingProgram);
                }
            }
        }

        /*[HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE TrainingProgram
                                            SET [Name] = @name,
                                                StartDate = @startDate,
                                                EndDate = @endDate,
                                                MaxAttendees = @maxAttendees
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));
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
                bool exists = await TrainingProgramExists(id);
                if (!exists)
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
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
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
                bool exists = await TrainingProgramExists(id);
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }*/

        private async Task<bool> TrainingProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], StartDate, EndDate, MaxAttendees
                        FROM TrainingProgram
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }
}