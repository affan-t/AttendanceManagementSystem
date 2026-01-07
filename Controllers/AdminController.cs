using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using System.Text;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(AttendanceManagementSystemContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // ADMIN DASHBOARD
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Summary Statistics
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalBatches = await _context.Batches.CountAsync();
            ViewBag.TotalSections = await _context.Sections.CountAsync();
            ViewBag.TotalAllocations = await _context.CourseAllocations.CountAsync();

            // Today's Attendance Overview
            var today = DateTime.Today;
            var todayAttendance = await _context.Attendances
                .Where(a => a.AttendanceDate.Date == today)
                .ToListAsync();

            ViewBag.TodayTotal = todayAttendance.Count;
            ViewBag.TodayPresent = todayAttendance.Count(a => a.IsPresent);
            ViewBag.TodayAbsent = todayAttendance.Count(a => !a.IsPresent);

            // Students below attendance threshold (75%)
            var lowAttendanceStudents = await GetLowAttendanceStudentsAsync(75);
            ViewBag.LowAttendanceCount = lowAttendanceStudents.Count;

            // Recent Activities
            var recentAttendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.CourseAllocation)
                .ThenInclude(ca => ca.Course)
                .OrderByDescending(a => a.AttendanceDate)
                .Take(10)
                .ToListAsync();
            ViewBag.RecentAttendance = recentAttendance;

            return View();
        }

        // ==========================================
        // ATTENDANCE MONITORING
        // ==========================================
        public async Task<IActionResult> AttendanceMonitoring(int? courseId, int? batchId, int? sectionId, DateTime? fromDate, DateTime? toDate)
        {
            // Load filter dropdowns
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Batches = await _context.Batches.ToListAsync();
            ViewBag.Sections = await _context.Sections.ToListAsync();

            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.CourseAllocation)
                .ThenInclude(ca => ca.Course)
                .Include(a => a.CourseAllocation)
                .ThenInclude(ca => ca.Teacher)
                .Include(a => a.CourseAllocation)
                .ThenInclude(ca => ca.Batch)
                .Include(a => a.CourseAllocation)
                .ThenInclude(ca => ca.Section)
                .AsQueryable();

            // Apply filters
            if (courseId.HasValue)
                query = query.Where(a => a.CourseAllocation.CourseId == courseId.Value);
            if (batchId.HasValue)
                query = query.Where(a => a.CourseAllocation.BatchId == batchId.Value);
            if (sectionId.HasValue)
                query = query.Where(a => a.CourseAllocation.SectionId == sectionId.Value);
            if (fromDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= toDate.Value);

            var attendanceRecords = await query
                .OrderByDescending(a => a.AttendanceDate)
                .Take(500)
                .ToListAsync();

            // Store current filter values
            ViewBag.SelectedCourseId = courseId;
            ViewBag.SelectedBatchId = batchId;
            ViewBag.SelectedSectionId = sectionId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(attendanceRecords);
        }

        // ==========================================
        // LOW ATTENDANCE STUDENTS
        // ==========================================
        public async Task<IActionResult> LowAttendanceStudents(int threshold = 75)
        {
            var lowAttendanceStudents = await GetLowAttendanceStudentsAsync(threshold);
            ViewBag.Threshold = threshold;
            return View(lowAttendanceStudents);
        }

        private async Task<List<dynamic>> GetLowAttendanceStudentsAsync(int threshold)
        {
            var students = await _context.Students
                .Include(s => s.Batch)
                .ToListAsync();

            var result = new List<dynamic>();

            foreach (var student in students)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id)
                    .ToListAsync();

                if (attendances.Count > 0)
                {
                    var presentCount = attendances.Count(a => a.IsPresent);
                    var percentage = (double)presentCount / attendances.Count * 100;

                    if (percentage < threshold)
                    {
                        result.Add(new
                        {
                            Student = student,
                            TotalClasses = attendances.Count,
                            PresentCount = presentCount,
                            AbsentCount = attendances.Count - presentCount,
                            Percentage = Math.Round(percentage, 1)
                        });
                    }
                }
            }

            return result.OrderBy(x => ((dynamic)x).Percentage).ToList();
        }

        // ==========================================
        // REPORTS
        // ==========================================
        public async Task<IActionResult> Reports()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Batches = await _context.Batches.ToListAsync();
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View();
        }

        // Course-wise Attendance Report
        public async Task<IActionResult> CourseAttendanceReport(int? courseId)
        {
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            if (!courseId.HasValue)
            {
                return View(new List<dynamic>());
            }

            var allocations = await _context.CourseAllocations
                .Include(ca => ca.Course)
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Batch)
                .Include(ca => ca.Section)
                .Where(ca => ca.CourseId == courseId.Value)
                .ToListAsync();

            var report = new List<dynamic>();

            foreach (var allocation in allocations)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.CourseAllocationId == allocation.Id)
                    .ToListAsync();

                var enrolledCount = await _context.Enrollments
                    .Where(e => e.CourseAllocationId == allocation.Id)
                    .CountAsync();

                report.Add(new
                {
                    Allocation = allocation,
                    EnrolledStudents = enrolledCount,
                    TotalClasses = attendances.Select(a => a.AttendanceDate.Date).Distinct().Count(),
                    TotalPresent = attendances.Count(a => a.IsPresent),
                    TotalAbsent = attendances.Count(a => !a.IsPresent),
                    AverageAttendance = attendances.Count > 0 
                        ? Math.Round((double)attendances.Count(a => a.IsPresent) / attendances.Count * 100, 1) 
                        : 0
                });
            }

            return View(report);
        }

        // Student-wise Attendance Report
        public async Task<IActionResult> StudentAttendanceReport(string searchTerm)
        {
            ViewBag.SearchTerm = searchTerm;

            var query = _context.Students
                .Include(s => s.Batch)
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || s.RollNo.Contains(searchTerm));
            }

            var students = await query.Take(50).ToListAsync();
            var report = new List<dynamic>();

            foreach (var student in students)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id)
                    .ToListAsync();

                var enrolledCourses = await _context.Enrollments
                    .Where(e => e.StudentId == student.Id)
                    .CountAsync();

                report.Add(new
                {
                    Student = student,
                    EnrolledCourses = enrolledCourses,
                    TotalClasses = attendances.Count,
                    PresentCount = attendances.Count(a => a.IsPresent),
                    AbsentCount = attendances.Count(a => !a.IsPresent),
                    Percentage = attendances.Count > 0 
                        ? Math.Round((double)attendances.Count(a => a.IsPresent) / attendances.Count * 100, 1) 
                        : 0
                });
            }

            return View(report);
        }

        // Teacher Activity Report
        public async Task<IActionResult> TeacherActivityReport()
        {
            var teachers = await _context.Teachers.ToListAsync();
            var report = new List<dynamic>();

            foreach (var teacher in teachers)
            {
                var allocations = await _context.CourseAllocations
                    .Include(ca => ca.Course)
                    .Where(ca => ca.TeacherId == teacher.Id)
                    .ToListAsync();

                var allocationIds = allocations.Select(a => a.Id).ToList();

                var attendanceMarked = await _context.Attendances
                    .Where(a => allocationIds.Contains(a.CourseAllocationId))
                    .Select(a => a.AttendanceDate.Date)
                    .Distinct()
                    .CountAsync();

                var totalStudentsEnrolled = await _context.Enrollments
                    .Where(e => allocationIds.Contains(e.CourseAllocationId))
                    .CountAsync();

                report.Add(new
                {
                    Teacher = teacher,
                    AssignedCourses = allocations.Count,
                    CourseNames = string.Join(", ", allocations.Select(a => a.Course.Code)),
                    TotalStudents = totalStudentsEnrolled,
                    AttendanceSessionsMarked = attendanceMarked
                });
            }

            return View(report);
        }

        // ==========================================
        // EXPORT REPORTS
        // ==========================================
        public async Task<IActionResult> ExportCourseReport(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var allocations = await _context.CourseAllocations
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Batch)
                .Include(ca => ca.Section)
                .Where(ca => ca.CourseId == courseId)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("\"Course Attendance Report\"");
            csv.AppendLine($"\"Course:\",\"{course.Name} ({course.Code})\"");
            csv.AppendLine($"\"Generated:\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
            csv.AppendLine($"\"Total Sections:\",\"{allocations.Count}\"");
            csv.AppendLine();
            csv.AppendLine("\"Teacher\",\"Batch\",\"Section\",\"Enrolled Students\",\"Total Sessions\",\"Total Present\",\"Total Absent\",\"Average Attendance %\"");

            foreach (var allocation in allocations)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.CourseAllocationId == allocation.Id)
                    .ToListAsync();

                var enrolledCount = await _context.Enrollments
                    .Where(e => e.CourseAllocationId == allocation.Id)
                    .CountAsync();

                var sessions = attendances.Select(a => a.AttendanceDate.Date).Distinct().Count();
                var presentCount = attendances.Count(a => a.IsPresent);
                var absentCount = attendances.Count(a => !a.IsPresent);
                var avgAttendance = attendances.Count > 0
                    ? Math.Round((double)presentCount / attendances.Count * 100, 1)
                    : 0;

                csv.AppendLine($"\"{EscapeCsvField(allocation.Teacher?.Name)}\",\"{EscapeCsvField(allocation.Batch?.Name)}\",\"{EscapeCsvField(allocation.Section?.Name)}\",\"{enrolledCount}\",\"{sessions}\",\"{presentCount}\",\"{absentCount}\",\"{avgAttendance}%\"");
            }

            var fileName = $"CourseReport_{course.Code}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var preamble = Encoding.UTF8.GetPreamble();
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var result = new byte[preamble.Length + csvBytes.Length];
            preamble.CopyTo(result, 0);
            csvBytes.CopyTo(result, preamble.Length);
            return File(result, "text/csv; charset=utf-8", fileName);
        }

        public async Task<IActionResult> ExportStudentReport(string searchTerm)
        {
            var query = _context.Students
                .Include(s => s.Batch)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || s.RollNo.Contains(searchTerm));
            }

            var students = await query.OrderBy(s => s.RollNo).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("\"Student Attendance Report\"");
            csv.AppendLine($"\"Generated:\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
            if (!string.IsNullOrEmpty(searchTerm))
            {
                csv.AppendLine($"\"Search Filter:\",\"{EscapeCsvField(searchTerm)}\"");
            }
            csv.AppendLine($"\"Total Students:\",\"{students.Count}\"");
            csv.AppendLine();
            csv.AppendLine("\"Roll No\",\"Name\",\"Father Name\",\"Batch\",\"Department\",\"Degree\",\"Total Classes\",\"Present\",\"Absent\",\"Attendance %\",\"Status\"");

            foreach (var student in students)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id)
                    .ToListAsync();

                var presentCount = attendances.Count(a => a.IsPresent);
                var absentCount = attendances.Count - presentCount;
                var percentage = attendances.Count > 0
                    ? Math.Round((double)presentCount / attendances.Count * 100, 1)
                    : 0;
                var status = percentage >= 75 ? "Good" : (percentage >= 60 ? "Warning" : "Critical");

                csv.AppendLine($"\"{EscapeCsvField(student.RollNo)}\",\"{EscapeCsvField(student.Name)}\",\"{EscapeCsvField(student.FatherName)}\",\"{EscapeCsvField(student.Batch?.Name ?? "N/A")}\",\"{EscapeCsvField(student.Department)}\",\"{EscapeCsvField(student.Degree)}\",\"{attendances.Count}\",\"{presentCount}\",\"{absentCount}\",\"{percentage}%\",\"{status}\"");
            }

            var fileName = $"StudentReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var preamble = Encoding.UTF8.GetPreamble();
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var result = new byte[preamble.Length + csvBytes.Length];
            preamble.CopyTo(result, 0);
            csvBytes.CopyTo(result, preamble.Length);
            return File(result, "text/csv; charset=utf-8", fileName);
        }

        public async Task<IActionResult> ExportTeacherReport()
        {
            var teachers = await _context.Teachers.OrderBy(t => t.Name).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("\"Teacher Activity Report\"");
            csv.AppendLine($"\"Generated:\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
            csv.AppendLine($"\"Total Teachers:\",\"{teachers.Count}\"");
            csv.AppendLine();
            csv.AppendLine("\"Name\",\"Department\",\"Designation\",\"Phone\",\"Assigned Courses\",\"Course Codes\",\"Total Students\",\"Attendance Sessions Marked\",\"Activity Status\"");

            foreach (var teacher in teachers)
            {
                var allocations = await _context.CourseAllocations
                    .Include(ca => ca.Course)
                    .Where(ca => ca.TeacherId == teacher.Id)
                    .ToListAsync();

                var allocationIds = allocations.Select(a => a.Id).ToList();

                var attendanceMarked = await _context.Attendances
                    .Where(a => allocationIds.Contains(a.CourseAllocationId))
                    .Select(a => a.AttendanceDate.Date)
                    .Distinct()
                    .CountAsync();

                var totalStudents = await _context.Enrollments
                    .Where(e => allocationIds.Contains(e.CourseAllocationId))
                    .CountAsync();

                var courseCodes = string.Join("; ", allocations.Select(a => a.Course?.Code ?? ""));
                var activityStatus = attendanceMarked > 10 ? "Active" : (attendanceMarked > 0 ? "Low Activity" : "Inactive");

                csv.AppendLine($"\"{EscapeCsvField(teacher.Name)}\",\"{EscapeCsvField(teacher.Department)}\",\"{EscapeCsvField(teacher.Designation)}\",\"{EscapeCsvField(teacher.PhoneNumber)}\",\"{allocations.Count}\",\"{EscapeCsvField(courseCodes)}\",\"{totalStudents}\",\"{attendanceMarked}\",\"{activityStatus}\"");
            }

            var fileName = $"TeacherReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var preamble = Encoding.UTF8.GetPreamble();
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var result = new byte[preamble.Length + csvBytes.Length];
            preamble.CopyTo(result, 0);
            csvBytes.CopyTo(result, preamble.Length);
            return File(result, "text/csv; charset=utf-8", fileName);
        }

        public async Task<IActionResult> ExportLowAttendanceReport(int threshold = 75)
        {
            var lowAttendanceStudents = await GetLowAttendanceStudentsAsync(threshold);

            var csv = new StringBuilder();
            csv.AppendLine("\"Low Attendance Students Report\"");
            csv.AppendLine($"\"Generated:\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
            csv.AppendLine($"\"Attendance Threshold:\",\"{threshold}%\"");
            csv.AppendLine($"\"Students Below Threshold:\",\"{lowAttendanceStudents.Count}\"");
            csv.AppendLine();
            csv.AppendLine("\"Roll No\",\"Name\",\"Batch\",\"Department\",\"Total Classes\",\"Present\",\"Absent\",\"Attendance %\",\"Below By\"");

            foreach (var item in lowAttendanceStudents)
            {
                var student = item.Student as Student;
                double percentage = item.Percentage;
                double belowBy = Math.Round(threshold - percentage, 1);

                csv.AppendLine($"\"{EscapeCsvField(student?.RollNo)}\",\"{EscapeCsvField(student?.Name)}\",\"{EscapeCsvField(student?.Batch?.Name ?? "N/A")}\",\"{EscapeCsvField(student?.Department)}\",\"{item.TotalClasses}\",\"{item.PresentCount}\",\"{item.AbsentCount}\",\"{percentage}%\",\"{belowBy}%\"");
            }

            var fileName = $"LowAttendanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var preamble = Encoding.UTF8.GetPreamble();
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var result = new byte[preamble.Length + csvBytes.Length];
            preamble.CopyTo(result, 0);
            csvBytes.CopyTo(result, preamble.Length);
            return File(result, "text/csv; charset=utf-8", fileName);
        }

        // Helper method to escape CSV fields
        private static string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        } 
    }
}