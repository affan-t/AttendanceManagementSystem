using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using System.Security.Claims;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class TimetablesController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;

        public TimetablesController(AttendanceManagementSystemContext context)
        {
            _context = context;
        }

        // GET: Timetables
        public async Task<IActionResult> Index()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Redirect students and teachers to their personalized schedule
            if (userRole == "Student" || userRole == "Teacher")
            {
                return RedirectToAction(nameof(MySchedule));
            }

            // Admin view - show all timetables
            var timetable = _context.TimetableEntries
                .Include(t => t.CourseAllocation)
                .ThenInclude(ca => ca.Course)
                .Include(t => t.CourseAllocation)
                .ThenInclude(ca => ca.Teacher)
                .Include(t => t.CourseAllocation)
                .ThenInclude(ca => ca.Section)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime);

            return View(await timetable.ToListAsync());
        }

        // GET: Timetables/MySchedule - For Students and Teachers
        public async Task<IActionResult> MySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            List<TimetableEntry> schedule;

            if (userRole == "Student")
            {
                // Get student's enrolled courses schedule
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                {
                    return NotFound("Student profile not found.");
                }

                schedule = await _context.TimetableEntries
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Course)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Teacher)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Section)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Batch)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Semester)
                    .Where(t => _context.Enrollments
                        .Any(e => e.StudentId == student.Id && e.CourseAllocationId == t.CourseAllocationId))
                    .OrderBy(t => t.DayOfWeek)
                    .ThenBy(t => t.StartTime)
                    .ToListAsync();

                ViewData["UserType"] = "Student";
            }
            else if (userRole == "Teacher")
            {
                // Get teacher's assigned courses schedule
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (teacher == null)
                {
                    return NotFound("Teacher profile not found.");
                }

                schedule = await _context.TimetableEntries
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Course)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Teacher)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Section)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Batch)
                    .Include(t => t.CourseAllocation)
                    .ThenInclude(ca => ca.Semester)
                    .Where(t => t.CourseAllocation.TeacherId == teacher.Id)
                    .OrderBy(t => t.DayOfWeek)
                    .ThenBy(t => t.StartTime)
                    .ToListAsync();

                ViewData["UserType"] = "Teacher";
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }

            return View(schedule);
        }

        // GET: Timetables/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Load courses nicely for the dropdown
            ViewData["CourseAllocationId"] = new SelectList(_context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Section)
                .Select(c => new { Id = c.Id, Name = c.Course.Name + " (" + c.Section.Name + ")" }),
                "Id", "Name");
            return View();
        }

        // POST: Timetables/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimetableEntry timetable)
        {
            // 1. Conflict Check: Is the Room busy at this time?
            bool conflict = await _context.TimetableEntries.AnyAsync(t =>
                t.DayOfWeek == timetable.DayOfWeek &&
                t.Room == timetable.Room &&
                ((timetable.StartTime >= t.StartTime && timetable.StartTime < t.EndTime) ||
                 (timetable.EndTime > t.StartTime && timetable.EndTime <= t.EndTime)));

            if (conflict)
            {
                ModelState.AddModelError("", $"Conflict! Room {timetable.Room} is already booked on {timetable.DayOfWeek} at this time.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(timetable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseAllocationId"] = new SelectList(_context.CourseAllocations
                .Include(c => c.Course)
                .Include(c => c.Section)
                .Select(c => new { Id = c.Id, Name = c.Course.Name + " (" + c.Section.Name + ")" }),
                "Id", "Name", timetable.CourseAllocationId);
            return View(timetable);
        }
    }
}