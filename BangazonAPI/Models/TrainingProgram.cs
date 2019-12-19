using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BangazonAPI.Models
{
    public class TrainingProgram
    {
        /*
            CREATE TABLE TrainingProgram (
                Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
                [Name] VARCHAR(255) NOT NULL,
                StartDate DATETIME NOT NULL,
                EndDate DATETIME NOT NULL,
                MaxAttendees INTEGER NOT NULL);
        */

        public int Id { get; set; }

        [Required(ErrorMessage = "Training Program Name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Training Program Name length should be between 1 and 255 characters")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Start Date of Training Program is required")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End Date of Training Program is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Maximum Number of Attendees is required for Training Program")]
        public int MaxAttendees { get; set; }
    }
}
