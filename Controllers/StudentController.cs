using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using System.Text;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(AttendanceManagementSystemContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper to get current student
        private async Task<Student?> GetCurrentStudentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.Students.Include(s => s.Batch).FirstOrDefaultAsync(s => s.UserId == user.Id);
        }

        // ==========================================
        // 1. DASHBOARD
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var student = await _context.Students
                .Include(s => s.Batch)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                return NotFound("Student profile not found. Please contact the Admin.");
            }

            var myCourses = await _context.Enrollments
                .Include(e => e.CourseAllocation).ThenInclude(c => c.Course)
                .Include(e => e.CourseAllocation).ThenInclude(c => c.Teacher)
                .Include(e => e.CourseAllocation).ThenInclude(c => c.Batch)
                .Include(e => e.CourseAllocation).ThenInclude(c => c.Section)
                .Include(e => e.CourseAllocation).ThenInclude(c => c.Semester)
                .Where(e => e.StudentId == student.Id)
                .ToListAsync();

            // Calculate overall attendance
            var allAttendance = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            var totalClasses = allAttendance.Count;
            var presentCount = allAttendance.Count(a => a.IsPresent);
            var overallPercentage = totalClasses > 0 ? Math.Round((double)presentCount / totalClasses * 100, 1) : 0;

            ViewBag.Student = student;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.PresentCount = presentCount;
            ViewBag.AbsentCount = totalClasses - presentCount;
            ViewBag.OverallPercentage = overallPercentage;

            // Per-course attendance
            var courseAttendance = new Dictionary<int, (int Total, int Present, double Percentage)>();
            foreach (var enrollment in myCourses)
            {
                var courseAtt = allAttendance.Where(a => a.CourseAllocationId == enrollment.CourseAllocationId).ToList();
                var cTotal = courseAtt.Count;
                var cPresent = courseAtt.Count(a => a.IsPresent);
                var cPercentage = cTotal > 0 ? Math.Round((double)cPresent / cTotal * 100, 1) : 0;
                courseAttendance[enrollment.CourseAllocationId] = (cTotal, cPresent, cPercentage);
            }
            ViewBag.CourseAttendance = courseAttendance;

            // Count courses below threshold
            var lowAttendanceCourses = courseAttendance.Values.Count(ca => ca.Total > 0 && ca.Percentage < 75);
            ViewBag.LowAttendanceCourses = lowAttendanceCourses;

            return View(myCourses);
        }

        // ==========================================
        // 2. PROFILE VIEW
        // ==========================================
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var student = await _context.Students
                .Include(s => s.Batch)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null) return NotFound("Student profile not found.");

            // Overall attendance stats
            var allAttendance = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            ViewBag.TotalClasses = allAttendance.Count;
            ViewBag.PresentCount = allAttendance.Count(a => a.IsPresent);
            ViewBag.AbsentCount = allAttendance.Count(a => !a.IsPresent);
            ViewBag.OverallPercentage = allAttendance.Count > 0 
                ? Math.Round((double)allAttendance.Count(a => a.IsPresent) / allAttendance.Count * 100, 1) 
                : 0;

            // Enrolled courses count
            ViewBag.EnrolledCourses = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .CountAsync();

            return View(student);
        }

        // ==========================================
        // 3. REGISTER COURSE
        // ==========================================
        public async Task<IActionResult> RegisterCourse()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null) return NotFound("Student profile not found.");

            var enrolledAllocationIds = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Select(e => e.CourseAllocationId)
                .ToListAsync();

            var availableCourses = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Where(c => !enrolledAllocationIds.Contains(c.Id))
                .Where(c => c.BatchId == student.BatchId)
                .ToListAsync();

            return View(availableCourses);
        }

        // ==========================================
        // 4. REGISTER (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null) return NotFound();

            var allocationToRegister = await _context.CourseAllocations
                .FirstOrDefaultAsync(a => a.Id == id);

            if (allocationToRegister == null) return NotFound();

            bool alreadyHasSubject = await _context.Enrollments
                .Include(e => e.CourseAllocation)
                .AnyAsync(e =>
                    e.StudentId == student.Id &&
                    e.CourseAllocation.CourseId == allocationToRegister.CourseId);

            if (alreadyHasSubject)
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(Index));
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseAllocationId = id,
                IsActive = true
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Successfully registered for the course!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. VIEW ATTENDANCE
        // ==========================================
        public async Task<IActionResult> ViewAttendance(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null) return NotFound();

            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Section)
                .Include(c => c.Batch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (allocation == null) return NotFound("Course not found.");

            var attendanceRecords = await _context.Attendances
                .Where(a => a.StudentId == student.Id && a.CourseAllocationId == id)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            int totalClasses = attendanceRecords.Count;
            int presentCount = attendanceRecords.Count(a => a.IsPresent);
            double percentage = totalClasses == 0 ? 0 : (double)presentCount / totalClasses * 100;

            ViewBag.Allocation = allocation;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.PresentCount = presentCount;
            ViewBag.AbsentCount = totalClasses - presentCount;
            ViewBag.Percentage = Math.Round(percentage, 1);

            return View(attendanceRecords);
        }

        // ==========================================
        // 6. DOWNLOAD ATTENDANCE (CSV)
        // ==========================================
        public async Task<IActionResult> DownloadAttendance(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null) return NotFound();

            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Section)
                .Include(c => c.Batch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (allocation == null) return NotFound("Course not found.");

            var attendanceRecords = await _context.Attendances
                .Where(a => a.StudentId == student.Id && a.CourseAllocationId == id)
                .OrderBy(a => a.AttendanceDate)
                .ToListAsync();

            int totalClasses = attendanceRecords.Count;
            int presentCount = attendanceRecords.Count(a => a.IsPresent);
            double percentage = totalClasses == 0 ? 0 : (double)presentCount / totalClasses * 100;

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Attendance Report");
            csv.AppendLine($"Student Name:,{student.Name}");
            csv.AppendLine($"Registration Number:,{student.RollNo}");
            csv.AppendLine($"Course:,{allocation.Course.Name} ({allocation.Course.Code})");
            csv.AppendLine($"Teacher:,{allocation.Teacher.Name}");
            csv.AppendLine($"Section:,{allocation.Section.Name}");
            csv.AppendLine($"Batch:,{allocation.Batch.Name}");
            csv.AppendLine($"Total Classes:,{totalClasses}");
            csv.AppendLine($"Present:,{presentCount}");
            csv.AppendLine($"Absent:,{totalClasses - presentCount}");
            csv.AppendLine($"Attendance Percentage:,{Math.Round(percentage, 2)}%");
            csv.AppendLine();
            csv.AppendLine("Date,Status");

            foreach (var record in attendanceRecords)
            {
                csv.AppendLine($"{record.AttendanceDate:yyyy-MM-dd},{(record.IsPresent ? "Present" : "Absent")}");
            }

            var fileName = $"Attendance_{allocation.Course.Code}_{student.RollNo}_{DateTime.Now:yyyyMMdd}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        // ==========================================
        // 7. ATTENDANCE SUMMARY (All Courses)
        // ==========================================
        public async Task<IActionResult> AttendanceSummary()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students
                .Include(s => s.Batch)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Course)
                .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Teacher)
                .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Section)
                .Where(e => e.StudentId == student.Id)
                .ToListAsync();

            var summary = new List<dynamic>();

            foreach (var enrollment in enrollments)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id && a.CourseAllocationId == enrollment.CourseAllocationId)
                    .ToListAsync();

                var total = attendances.Count;
                var present = attendances.Count(a => a.IsPresent);
                var percentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;

                summary.Add(new
                {
                    Enrollment = enrollment,
                    TotalClasses = total,
                    PresentCount = present,
                    AbsentCount = total - present,
                    Percentage = percentage
                });
            }

            ViewBag.Student = student;
                        return View(summary.OrderBy(s => ((dynamic)s).Percentage).ToList());
                    }

                    // ==========================================
                    // 8. MY ATTENDANCE REPORT (with Charts)
                    // ==========================================
                    public async Task<IActionResult> MyReport()
                    {
                        var student = await GetCurrentStudentAsync();
                        if (student == null) return NotFound("Student profile not found.");

                        var enrollments = await _context.Enrollments
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Course)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Teacher)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Section)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Semester)
                            .Where(e => e.StudentId == student.Id)
                            .ToListAsync();

                        var reportData = new List<dynamic>();
                        var totalPresent = 0;
                        var totalAbsent = 0;

                        foreach (var enrollment in enrollments)
                        {
                            var attendances = await _context.Attendances
                                .Where(a => a.StudentId == student.Id && a.CourseAllocationId == enrollment.CourseAllocationId)
                                .ToListAsync();

                            var total = attendances.Count;
                            var present = attendances.Count(a => a.IsPresent);
                            var absent = total - present;
                            var percentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;

                            totalPresent += present;
                            totalAbsent += absent;

                            reportData.Add(new
                            {
                                Enrollment = enrollment,
                                TotalClasses = total,
                                PresentCount = present,
                                AbsentCount = absent,
                                Percentage = percentage
                            });
                        }

                        // Get attendance by date for trend chart
                        var attendanceByDate = await _context.Attendances
                            .Where(a => a.StudentId == student.Id)
                            .GroupBy(a => a.AttendanceDate.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                Present = g.Count(a => a.IsPresent),
                                Absent = g.Count(a => !a.IsPresent)
                            })
                            .OrderBy(x => x.Date)
                            .Take(30) // Last 30 entries
                            .ToListAsync();

                        ViewBag.Student = student;
                        ViewBag.TotalPresent = totalPresent;
                        ViewBag.TotalAbsent = totalAbsent;
                        ViewBag.AttendanceByDate = attendanceByDate;

                        return View(reportData);
                    }

                    // ==========================================
                    // 9. EXPORT FULL REPORT (CSV)
                    // ==========================================
                    public async Task<IActionResult> ExportFullReport()
                    {
                        var student = await GetCurrentStudentAsync();
                        if (student == null) return NotFound("Student profile not found.");

                        var enrollments = await _context.Enrollments
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Course)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Teacher)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Section)
                            .Include(e => e.CourseAllocation).ThenInclude(ca => ca.Semester)
                            .Where(e => e.StudentId == student.Id)
                            .ToListAsync();

                        var csv = new StringBuilder();
                        csv.AppendLine($"Attendance Report - {student.Name}");
                        csv.AppendLine($"Roll No: {student.RollNo}");
                        csv.AppendLine($"Batch: {student.Batch?.Name}");
                        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        csv.AppendLine();
                        csv.AppendLine("Course Code,Course Name,Teacher,Section,Semester,Total,Present,Absent,Attendance %,Status");

                        var totalPresent = 0;
                        var totalAbsent = 0;

                        foreach (var enrollment in enrollments)
                        {
                            var attendances = await _context.Attendances
                                .Where(a => a.StudentId == student.Id && a.CourseAllocationId == enrollment.CourseAllocationId)
                                .ToListAsync();

                            var total = attendances.Count;
                            var present = attendances.Count(a => a.IsPresent);
                            var absent = total - present;
                            var percentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;
                            var status = percentage >= 75 ? "Good" : (percentage >= 60 ? "Warning" : "Critical");

                            totalPresent += present;
                            totalAbsent += absent;

                            csv.AppendLine($"{enrollment.CourseAllocation.Course.Code},{enrollment.CourseAllocation.Course.Name},{enrollment.CourseAllocation.Teacher.Name},{enrollment.CourseAllocation.Section.Name},{enrollment.CourseAllocation.Semester.Name},{total},{present},{absent},{percentage}%,{status}");
                        }

                        var overallTotal = totalPresent + totalAbsent;
                        var overallPercentage = overallTotal > 0 ? Math.Round((double)totalPresent / overallTotal * 100, 1) : 0;
                        csv.AppendLine();
                        csv.AppendLine($"Overall,,,,,{overallTotal},{totalPresent},{totalAbsent},{overallPercentage}%,{(overallPercentage >= 75 ? "Good" : (overallPercentage >= 60 ? "Warning" : "Critical"))}");

                        var fileName = $"AttendanceReport_{student.RollNo}_{DateTime.Now:yyyyMMdd}.csv";
                        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
                    }
                }
            }