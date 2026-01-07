using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public int CreditHours { get; set; }
    }
}