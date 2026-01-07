using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.ViewModels
{
    public class StudentCreateViewModel
    {
        // --- Personal & Login (No Changes) ---
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public string CNIC { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }

        // --- Academic Details ---
        [Required]
        public string Department { get; set; }
        [Required]
        public string Degree { get; set; }
        [Required]
        public int EnrollmentYear { get; set; }
        [Required]
        public string RollNo { get; set; }

        // === UPDATED: Only "Batch" (Fall/Spring) ===
        [Required]
        [Display(Name = "Intake Batch")] // Display as "Intake Batch"
        public string IntakeBatch { get; set; }

        // === Batch/Session Assignment ===
        [Required]
        [Display(Name = "Assign to Batch/Session")]
        public int BatchId { get; set; }
    }
}