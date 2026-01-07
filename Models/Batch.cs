using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    public class Batch
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., 2022-2026

        public string Year { get; set; }
    }
}