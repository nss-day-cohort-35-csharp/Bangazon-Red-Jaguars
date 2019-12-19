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
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First Name length should be between 1 and 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last Name length should be between 1 and 50 characters")]
        public string LastName { get; set; }
        public int DepartmetId { get; set; }
        public string Email { get; set; }
        public bool IsSupervisor { get; set; } = false;
        public int ComputerId { get; set; }
    }
}
