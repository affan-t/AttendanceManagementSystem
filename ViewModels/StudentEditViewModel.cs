using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.ViewModels
{
    public class StudentEditViewModel
    {
        public int Id { get; set; }

        // --- Login Details ---
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? Password { get; set; } // Optional: Leave blank to keep existing

        // --- Personal Details ---
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }

        [Required]
        public string CNIC { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        // --- Academic Details ---
        [Required]
        public string Department { get; set; }

        [Required]
        public string Degree { get; set; }

        [Required]
        [Display(Name = "Year of Enrollment")]
        public int EnrollmentYear { get; set; }

        [Required]
        [Display(Name = "Registration Number")]
        public string RollNo { get; set; }

        [Required]
        [Display(Name = "Batch")]
        public string IntakeBatch { get; set; } // Fall/Spring

        // === Batch/Session Assignment ===
        [Display(Name = "Assign to Batch/Session")]
        public int? BatchId { get; set; }
    }
}