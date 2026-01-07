using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Models;

namespace AttendanceManagementSystem.Data
{
    public class AttendanceManagementSystemContext : IdentityDbContext
    {
        public AttendanceManagementSystemContext(DbContextOptions<AttendanceManagementSystemContext> options)
            : base(options)
        {
        }

        // 1. Basic Academic Tables
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Section> Sections { get; set; }

        // 2. Linking Tables
        public DbSet<CourseAllocation> CourseAllocations { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<TimetableEntry> TimetableEntries { get; set; }
        // 3. Database Configuration (Rules)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Rule Set 1: Enrollments ===
            // Prevent deleting a Student from automatically deleting their enrollment history
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseAllocation)
                .WithMany()
                .HasForeignKey(e => e.CourseAllocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Rule Set 2: Attendance ===
            // Prevent deleting a Student from automatically deleting their attendance records
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.CourseAllocation)
                .WithMany()
                .HasForeignKey(a => a.CourseAllocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}