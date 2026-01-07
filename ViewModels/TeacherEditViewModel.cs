using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.ViewModels
{
    public class TeacherEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? Password { get; set; } // Optional (Nullable)

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;
    }
}