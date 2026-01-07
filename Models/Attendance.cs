using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        // Links to the specific class session (Teacher + Course + Batch)
        [Required]
        public int CourseAllocationId { get; set; }
        [ForeignKey("CourseAllocationId")]
        public virtual CourseAllocation? CourseAllocation { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public bool IsPresent { get; set; } // true = Present, false = Absent
    }
}