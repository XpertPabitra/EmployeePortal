using System.ComponentModel.DataAnnotations;

namespace EmployeePortal.Models.Domain
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public string Designation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Please select an employee type")]
        public int EmployeeTypeId { get; set; }

        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        // FIX: The '?' allows this to be null if the database column is empty
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Hire Date is required")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Range(0, 999999999, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;
    }
}