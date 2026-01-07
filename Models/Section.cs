using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    public class Section
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., Section A
    }
}