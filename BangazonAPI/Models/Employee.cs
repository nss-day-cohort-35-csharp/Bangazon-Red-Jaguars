using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BangazonAPI.Models
{
    public class Student
    {
        /*
            CREATE TABLE Employee (
	            Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	            FirstName VARCHAR(55) NOT NULL,
	            LastName VARCHAR(55) NOT NULL,
	            DepartmentId INTEGER NOT NULL,
	            Email VARCHAR(55) NOT NULL,
	            IsSupervisor BIT NOT NULL DEFAULT(0),
	            ComputerId INTEGER NOT NULL,
                CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id),
	            CONSTRAINT FK_EmployeeComputer FOREIGN KEY (ComputerId) REFERENCES Computer(Id));
        */

        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "First Name length should be between 1 and 55 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(55, MinimumLength = 1, ErrorMessage = "Last Name length should be between 1 and 55 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Employee eMail is required")]
        [StringLength(55, MinimumLength = 5, ErrorMessage = "eMail length should be between 5 and 55 characters")]
        public string Email { get; set; }
        
        [Required]
        public bool IsSupervisor { get; set; } = false;

        [Required(ErrorMessage = "Computer Id is required")]
        public int ComputerId { get; set; }
    }
}
