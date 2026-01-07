using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // Link to the Student
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        // Link to the specific Class (Course + Teacher + Batch + Section)
        [Required]
        public int CourseAllocationId { get; set; }
        [ForeignKey("CourseAllocationId")]
        public virtual CourseAllocation? CourseAllocation { get; set; }

        public bool IsActive { get; set; } = true;
    }
}