using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using System.Text;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeacherController(AttendanceManagementSystemContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper method to get teacher
        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
        }

        // Helper method to get teacher's allocations
        private async Task<List<CourseAllocation>> GetTeacherAllocationsAsync(int teacherId)
        {
            return await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        // ==========================================
        // 1. DASHBOARD
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            // Fetch courses assigned to this teacher
            var myClasses = await GetTeacherAllocationsAsync(teacher.Id);

            // Today's classes from timetable
            var today = DateTime.Now.DayOfWeek.ToString();
            var todaysClasses = await _context.TimetableEntries
                .Include(t => t.CourseAllocation)
                .ThenInclude(ca => ca.Course)
                .Include(t => t.CourseAllocation)
                .ThenInclude(ca => ca.Section)
                .Where(t => t.CourseAllocation.TeacherId == teacher.Id && t.DayOfWeek == today)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            ViewBag.TodaysClasses = todaysClasses;
            ViewBag.Teacher = teacher;

            // Check today's attendance status for each allocation
            var allocationIds = myClasses.Select(c => c.Id).ToList();
            var todayAttendance = await _context.Attendances
                .Where(a => allocationIds.Contains(a.CourseAllocationId) && a.AttendanceDate.Date == DateTime.Today)
                .Select(a => a.CourseAllocationId)
                .Distinct()
                .ToListAsync();
            ViewBag.TodayAttendanceMarked = todayAttendance;

            // Low attendance students alert
            var lowAttendanceCount = 0;
            foreach (var allocation in myClasses)
            {
                var enrolledStudents = await _context.Enrollments
                    .Where(e => e.CourseAllocationId == allocation.Id)
                    .Select(e => e.StudentId)
                    .ToListAsync();

                foreach (var studentId in enrolledStudents)
                {
                    var attendances = await _context.Attendances
                        .Where(a => a.StudentId == studentId && a.CourseAllocationId == allocation.Id)
                        .ToListAsync();

                    if (attendances.Count > 0)
                    {
                        var percentage = (double)attendances.Count(a => a.IsPresent) / attendances.Count * 100;
                        if (percentage < 75) lowAttendanceCount++;
                    }
                }
            }
            ViewBag.LowAttendanceCount = lowAttendanceCount;

            return View(myClasses);
        }

        // ==========================================
        // COURSE SELECTION PAGES (for sidebar links)
        // ==========================================
        
        // Select course for Mark Attendance
        public async Task<IActionResult> MarkAttendance(int? id)
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound("Teacher profile not found.");

            // If no ID provided, show course selection
            if (!id.HasValue)
            {
                var allocations = await GetTeacherAllocationsAsync(teacher.Id);
                ViewBag.ActionTitle = "Mark Attendance";
                ViewBag.ActionDescription = "Select a course to mark attendance";
                ViewBag.ActionName = "MarkAttendance";
                ViewBag.ActionIcon = "bi-check2-square";
                ViewBag.ActionBtnClass = "btn-success";
                return View("SelectCourse", allocations);
            }

            // Proceed with existing logic
            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (allocation == null || allocation.TeacherId != teacher.Id)
            {
                return Unauthorized();
            }

            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseAllocationId == id.Value)
                .OrderBy(e => e.Student.RollNo)
                .Select(e => e.Student)
                .ToListAsync();

            // Check if attendance already marked for today
            var todayAttendance = await _context.Attendances
                .Where(a => a.CourseAllocationId == id.Value && a.AttendanceDate.Date == DateTime.Today)
                .ToListAsync();

            ViewBag.Allocation = allocation;
            ViewBag.TodayAttendance = todayAttendance;
            ViewBag.AttendanceAlreadyMarked = todayAttendance.Count > 0;
            return View("MarkAttendanceForm", students);
        }

        // Select course for Attendance History
        public async Task<IActionResult> AttendanceHistory(int? id)
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound("Teacher profile not found.");

            // If no ID provided, show course selection
            if (!id.HasValue)
            {
                var allocations = await GetTeacherAllocationsAsync(teacher.Id);
                ViewBag.ActionTitle = "Attendance History";
                ViewBag.ActionDescription = "Select a course to view attendance history";
                ViewBag.ActionName = "AttendanceHistory";
                ViewBag.ActionIcon = "bi-clock-history";
                ViewBag.ActionBtnClass = "btn-primary";
                return View("SelectCourse", allocations);
            }

            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (allocation == null || allocation.TeacherId != teacher.Id)
            {
                return Unauthorized();
            }

            var attendanceRecords = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseAllocationId == id.Value)
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.RollNo)
                .ToListAsync();

            // Group by date
            var groupedAttendance = attendanceRecords
                .GroupBy(a => a.AttendanceDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.Allocation = allocation;
            ViewBag.GroupedAttendance = groupedAttendance;
            return View("AttendanceHistoryView", attendanceRecords);
        }

        // Select course for View Students
        public async Task<IActionResult> ViewStudents(int? id)
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound("Teacher profile not found.");

            // If no ID provided, show course selection
            if (!id.HasValue)
            {
                var allocations = await GetTeacherAllocationsAsync(teacher.Id);
                ViewBag.ActionTitle = "My Students";
                ViewBag.ActionDescription = "Select a course to view enrolled students";
                ViewBag.ActionName = "ViewStudents";
                ViewBag.ActionIcon = "bi-people";
                ViewBag.ActionBtnClass = "btn-info";
                return View("SelectCourse", allocations);
            }

            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (allocation == null || allocation.TeacherId != teacher.Id)
            {
                return Unauthorized();
            }

            var enrolledStudents = await _context.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.Batch)
                .Where(e => e.CourseAllocationId == id.Value)
                .OrderBy(e => e.Student.RollNo)
                .ToListAsync();

            // Calculate attendance for each student
            var studentAttendance = new Dictionary<int, (int Total, int Present, double Percentage)>();
            foreach (var enrollment in enrolledStudents)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == enrollment.StudentId && a.CourseAllocationId == id.Value)
                    .ToListAsync();

                var total = attendances.Count;
                var present = attendances.Count(a => a.IsPresent);
                var percentage = total > 0 ? (double)present / total * 100 : 0;
                studentAttendance[enrollment.StudentId] = (total, present, Math.Round(percentage, 1));
            }

            ViewBag.Allocation = allocation;
            ViewBag.StudentAttendance = studentAttendance;
            return View("ViewStudentsPage", enrolledStudents);
        }

        // Select course for Low Attendance Students
        public async Task<IActionResult> LowAttendanceStudents(int? id)
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound("Teacher profile not found.");

            // If no ID provided, show course selection
            if (!id.HasValue)
            {
                var allocations = await GetTeacherAllocationsAsync(teacher.Id);
                ViewBag.ActionTitle = "Low Attendance Students";
                ViewBag.ActionDescription = "Select a course to view students with low attendance";
                ViewBag.ActionName = "LowAttendanceStudents";
                ViewBag.ActionIcon = "bi-exclamation-triangle";
                ViewBag.ActionBtnClass = "btn-danger";
                return View("SelectCourse", allocations);
            }

            var allocation = await _context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Batch)
                .Include(c => c.Section)
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (allocation == null || allocation.TeacherId != teacher.Id)
            {
                return Unauthorized();
            }

            var enrolledStudents = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseAllocationId == id.Value)
                .ToListAsync();

            var lowAttendanceStudents = new List<dynamic>();

            foreach (var enrollment in enrolledStudents)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == enrollment.StudentId && a.CourseAllocationId == id.Value)
                    .ToListAsync();

                if (attendances.Count > 0)
                {
                    var present = attendances.Count(a => a.IsPresent);
                    var percentage = (double)present / attendances.Count * 100;

                    if (percentage < 75)
                    {
                        lowAttendanceStudents.Add(new
                        {
                            Student = enrollment.Student,
                            TotalClasses = attendances.Count,
                            PresentCount = present,
                            AbsentCount = attendances.Count - present,
                            Percentage = Math.Round(percentage, 1)
                        });
                    }
                }
            }

            ViewBag.Allocation = allocation;
                        return View("LowAttendanceStudentsView", lowAttendanceStudents.OrderBy(s => ((dynamic)s).Percentage).ToList());
                    }

                    // ==========================================
                    // SAVE ATTENDANCE
                    // ==========================================
                    [HttpPost]
                    [ValidateAntiForgeryToken]
                    public async Task<IActionResult> SaveAttendance(int allocationId, DateTime date, Dictionary<int, bool> attendanceStatus)
                    {
                        var teacher = await GetCurrentTeacherAsync();
                        if (teacher == null) return NotFound("Teacher profile not found.");

                        // Security check
                        var allocation = await _context.CourseAllocations.FindAsync(allocationId);
                        if (allocation == null || allocation.TeacherId != teacher.Id)
                        {
                            return Unauthorized();
                        }

                        // Check for duplicate attendance
                        var existingAttendance = await _context.Attendances
                            .Where(a => a.CourseAllocationId == allocationId && a.AttendanceDate.Date == date.Date)
                            .ToListAsync();

                        if (existingAttendance.Any())
                        {
                            TempData["Error"] = "Attendance for this date has already been marked!";
                            return RedirectToAction(nameof(MarkAttendance), new { id = allocationId });
                        }

                        foreach (var studentId in attendanceStatus.Keys)
                        {
                            var isPresent = attendanceStatus[studentId];

                            var attendance = new Attendance
                            {
                                CourseAllocationId = allocationId,
                                AttendanceDate = date,
                                StudentId = studentId,
                                IsPresent = isPresent
                            };
                            _context.Attendances.Add(attendance);
                        }
                        await _context.SaveChangesAsync();

                        TempData["Message"] = "Attendance Marked Successfully!";
                        return RedirectToAction(nameof(Index));
                    }

                    // ==========================================
                    // EDIT ATTENDANCE
                    // ==========================================
                    [HttpGet]
                    public async Task<IActionResult> EditAttendance(int id, DateTime date)
                    {
                        var teacher = await GetCurrentTeacherAsync();
                        if (teacher == null) return NotFound("Teacher profile not found.");

                        var allocation = await _context.CourseAllocations
                            .Include(c => c.Course)
                            .Include(c => c.Batch)
                            .Include(c => c.Section)
                            .FirstOrDefaultAsync(a => a.Id == id);

                        if (allocation == null || allocation.TeacherId != teacher.Id)
                        {
                            return Unauthorized();
                        }

                        var attendanceRecords = await _context.Attendances
                            .Include(a => a.Student)
                            .Where(a => a.CourseAllocationId == id && a.AttendanceDate.Date == date.Date)
                            .OrderBy(a => a.Student.RollNo)
                            .ToListAsync();

                        if (!attendanceRecords.Any())
                        {
                            TempData["Error"] = "No attendance records found for this date.";
                            return RedirectToAction(nameof(AttendanceHistory), new { id });
                        }

                        ViewBag.Allocation = allocation;
                        ViewBag.Date = date;
                        return View(attendanceRecords);
                    }

                    [HttpPost]
                    [ValidateAntiForgeryToken]
                    public async Task<IActionResult> UpdateAttendance(int allocationId, DateTime date, Dictionary<int, bool> attendanceStatus)
                    {
                        var teacher = await GetCurrentTeacherAsync();
                        if (teacher == null) return NotFound("Teacher profile not found.");

                        var allocation = await _context.CourseAllocations.FindAsync(allocationId);
                        if (allocation == null || allocation.TeacherId != teacher.Id)
                        {
                            return Unauthorized();
                        }

                        foreach (var studentId in attendanceStatus.Keys)
                        {
                            var record = await _context.Attendances
                                .FirstOrDefaultAsync(a => a.CourseAllocationId == allocationId 
                                    && a.StudentId == studentId 
                                    && a.AttendanceDate.Date == date.Date);

                            if (record != null)
                            {
                                record.IsPresent = attendanceStatus[studentId];
                                _context.Update(record);
                            }
                        }

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Attendance Updated Successfully!";
                        return RedirectToAction(nameof(AttendanceHistory), new { id = allocationId });
                    }

                    // ==========================================
                    // ATTENDANCE REPORT (with Charts)
                    // ==========================================
                    public async Task<IActionResult> AttendanceReport(int? id)
                    {
                        var teacher = await GetCurrentTeacherAsync();
                        if (teacher == null) return NotFound("Teacher profile not found.");

                        // If no ID provided, show course selection
                        if (!id.HasValue)
                        {
                            var allocations = await GetTeacherAllocationsAsync(teacher.Id);
                            ViewBag.ActionTitle = "Attendance Report";
                            ViewBag.ActionDescription = "Select a course to generate attendance report";
                            ViewBag.ActionName = "AttendanceReport";
                            ViewBag.ActionIcon = "bi-file-earmark-bar-graph";
                            ViewBag.ActionBtnClass = "btn-primary";
                            return View("SelectCourse", allocations);
                        }

                        var allocation = await _context.CourseAllocations
                            .Include(c => c.Course)
                            .Include(c => c.Batch)
                            .Include(c => c.Section)
                            .Include(c => c.Semester)
                            .Include(c => c.Teacher)
                            .FirstOrDefaultAsync(a => a.Id == id.Value);

                        if (allocation == null || allocation.TeacherId != teacher.Id)
                        {
                            return Unauthorized();
                        }

                        var enrolledStudents = await _context.Enrollments
                            .Include(e => e.Student)
                            .Where(e => e.CourseAllocationId == id.Value)
                            .OrderBy(e => e.Student.RollNo)
                            .ToListAsync();

                        var reportData = new List<dynamic>();
                        var totalPresent = 0;
                        var totalAbsent = 0;

                        foreach (var enrollment in enrolledStudents)
                        {
                            var attendances = await _context.Attendances
                                .Where(a => a.StudentId == enrollment.StudentId && a.CourseAllocationId == id.Value)
                                .ToListAsync();

                            var total = attendances.Count;
                            var present = attendances.Count(a => a.IsPresent);
                            var absent = total - present;
                            var percentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;

                            totalPresent += present;
                            totalAbsent += absent;

                            reportData.Add(new
                            {
                                Student = enrollment.Student,
                                TotalClasses = total,
                                PresentCount = present,
                                AbsentCount = absent,
                                Percentage = percentage
                            });
                        }

                        // Get attendance by date for chart
                        var attendanceByDate = await _context.Attendances
                            .Where(a => a.CourseAllocationId == id.Value)
                            .GroupBy(a => a.AttendanceDate.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                Present = g.Count(a => a.IsPresent),
                                Absent = g.Count(a => !a.IsPresent)
                            })
                            .OrderBy(x => x.Date)
                            .ToListAsync();

                        ViewBag.Allocation = allocation;
                        ViewBag.TotalPresent = totalPresent;
                        ViewBag.TotalAbsent = totalAbsent;
                        ViewBag.AttendanceByDate = attendanceByDate;
                        ViewBag.Teacher = teacher;

                        return View(reportData);
                    }

                    // ==========================================
                    // EXPORT REPORT TO CSV
                    // ==========================================
                    public async Task<IActionResult> ExportAttendanceReport(int id)
                    {
                        var teacher = await GetCurrentTeacherAsync();
                        if (teacher == null) return NotFound("Teacher profile not found.");

                        var allocation = await _context.CourseAllocations
                            .Include(c => c.Course)
                            .Include(c => c.Batch)
                            .Include(c => c.Section)
                            .Include(c => c.Semester)
                            .FirstOrDefaultAsync(a => a.Id == id);

                        if (allocation == null || allocation.TeacherId != teacher.Id)
                        {
                            return Unauthorized();
                        }

                        var enrolledStudents = await _context.Enrollments
                            .Include(e => e.Student)
                            .Where(e => e.CourseAllocationId == id)
                            .OrderBy(e => e.Student.RollNo)
                            .ToListAsync();

                        var csv = new StringBuilder();
                        csv.AppendLine($"Attendance Report - {allocation.Course.Name} ({allocation.Course.Code})");
                        csv.AppendLine($"Teacher: {teacher.Name}");
                        csv.AppendLine($"Batch: {allocation.Batch.Name} | Section: {allocation.Section.Name} | Semester: {allocation.Semester.Name}");
                        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        csv.AppendLine();
                        csv.AppendLine("Roll No,Student Name,Total Classes,Present,Absent,Attendance %,Status");

                        foreach (var enrollment in enrolledStudents)
                        {
                            var attendances = await _context.Attendances
                                .Where(a => a.StudentId == enrollment.StudentId && a.CourseAllocationId == id)
                                .ToListAsync();

                            var total = attendances.Count;
                            var present = attendances.Count(a => a.IsPresent);
                            var percentage = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;
                            var status = percentage >= 75 ? "Good" : (percentage >= 60 ? "Warning" : "Critical");

                            csv.AppendLine($"{enrollment.Student.RollNo},{enrollment.Student.Name},{total},{present},{total - present},{percentage}%,{status}");
                        }

                        var fileName = $"AttendanceReport_{allocation.Course.Code}_{DateTime.Now:yyyyMMdd}.csv";
                        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
                    }
                }
            }