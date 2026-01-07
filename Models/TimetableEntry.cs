using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    public class TimetableEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseAllocationId { get; set; }
        [ForeignKey("CourseAllocationId")]
        public virtual CourseAllocation? CourseAllocation { get; set; }

        [Required]
        public string DayOfWeek { get; set; } // Monday, Tuesday...

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string Room { get; set; }
    }
}