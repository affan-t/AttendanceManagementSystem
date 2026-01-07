using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    public class Semester
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Semester Name")]
        public string Name { get; set; } // e.g., Fall 2025

        [Display(Name = "Active Semester")]
        public bool IsActive { get; set; }
    }
}