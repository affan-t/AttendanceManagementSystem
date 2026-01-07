using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    public class CourseAllocation
    {
        [Key]
        public int Id { get; set; }

        // 1. Who is teaching?
        [Required]
        [Display(Name = "Teacher")]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        // ADD THE '?' HERE
        public virtual Teacher? Teacher { get; set; }

        // 2. What subject?
        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        // ADD THE '?' HERE
        public virtual Course? Course { get; set; }

        // 3. To which Batch?
        [Required]
        [Display(Name = "Batch / Session")]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        // ADD THE '?' HERE
        public virtual Batch? Batch { get; set; }

        // 4. Which Section?
        [Required]
        [Display(Name = "Section")]
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        // ADD THE '?' HERE
        public virtual Section? Section { get; set; }

        // 5. Which Semester?
        [Required]
        [Display(Name = "Semester")]
        public int SemesterId { get; set; }
        [ForeignKey("SemesterId")]
        // ADD THE '?' HERE
        public virtual Semester? Semester { get; set; }
    }
}