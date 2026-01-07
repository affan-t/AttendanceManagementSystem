using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; } // NEW

        [Required]
        public string CNIC { get; set; } // NEW

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } // NEW

        [Required]
        public string Address { get; set; } // NEW

        [Required]
        public string Department { get; set; }

        [Required]
        public string Degree { get; set; } // NEW (e.g., BSCS, BBA)

        [Required]
        [Display(Name = "Intake Batch")]
        public string IntakeBatch { get; set; } // NEW (e.g., Fall 2025)

        [Required]
        [Display(Name = "Year of Enrollment")]
        public int EnrollmentYear { get; set; } // NEW

        [Required]
        [Display(Name = "Registration Number")]
        public string RollNo { get; set; } // Maps to "Registration Number"

        public int? BatchId { get; set; }
        [ForeignKey("BatchId")]
        public virtual Batch? Batch { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }
    }
}