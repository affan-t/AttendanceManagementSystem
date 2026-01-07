using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using System.IO;
using AttendanceManagementSystem.ViewModels;

namespace AttendanceManagementSystem.Controllers
{
    public class CourseAllocationsController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CourseAllocationsController(AttendanceManagementSystemContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CourseAllocations
        public async Task<IActionResult> Index()
        {
            var attendanceManagementSystemContext = _context.CourseAllocations.Include(c => c.Batch).Include(c => c.Course).Include(c => c.Section).Include(c => c.Semester).Include(c => c.Teacher);
            return View(await attendanceManagementSystemContext.ToListAsync());
        }

        // GET: CourseAllocations/GetTableData - AJAX
        public async Task<IActionResult> GetTableData()
        {
            var allocations = await _context.CourseAllocations
                .Include(c => c.Batch)
                .Include(c => c.Course)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Include(c => c.Teacher)
                .ToListAsync();
            return PartialView("_AllocationsTable", allocations);
        }

        // GET: CourseAllocations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAllocation = await _context.CourseAllocations
                .Include(c => c.Batch)
                .Include(c => c.Course)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseAllocation == null)
            {
                return NotFound();
            }

            return View(courseAllocation);
        }

        // GET: CourseAllocations/Create
        public IActionResult Create()
        {
            ViewData["BatchId"] = new SelectList(_context.Batches, "Id", "Name");
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code");
            ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Name");
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateModal");
            }
            return View();
        }

        // POST: CourseAllocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeacherId,CourseId,BatchId,SectionId,SemesterId")] CourseAllocation courseAllocation)
        {
            // Validation: Prevent duplicate course-section assignment
            var existingAllocation = await _context.CourseAllocations
                .FirstOrDefaultAsync(ca => ca.CourseId == courseAllocation.CourseId 
                    && ca.SectionId == courseAllocation.SectionId
                    && ca.SemesterId == courseAllocation.SemesterId
                    && ca.BatchId == courseAllocation.BatchId);

            if (existingAllocation != null)
            {
                ModelState.AddModelError("", "This course section is already assigned to another teacher. Each section of a course can only be assigned to one teacher.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(courseAllocation);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course allocation created successfully!" });
                }
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            ViewData["BatchId"] = new SelectList(_context.Batches, "Id", "Name", courseAllocation.BatchId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", courseAllocation.CourseId);
            ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Name", courseAllocation.SectionId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", courseAllocation.SemesterId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name", courseAllocation.TeacherId);
            return View(courseAllocation);
        }

        // GET: CourseAllocations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAllocation = await _context.CourseAllocations.FindAsync(id);
            if (courseAllocation == null)
            {
                return NotFound();
            }
            ViewData["BatchId"] = new SelectList(_context.Batches, "Id", "Name", courseAllocation.BatchId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", courseAllocation.CourseId);
            ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Name", courseAllocation.SectionId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", courseAllocation.SemesterId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name", courseAllocation.TeacherId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditModal", courseAllocation);
            }

            return View(courseAllocation);
        }

        // POST: CourseAllocations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,CourseId,BatchId,SectionId,SemesterId")] CourseAllocation courseAllocation)
        {
            if (id != courseAllocation.Id)
            {
                return NotFound();
            }

            // Validation: Prevent duplicate course-section assignment
            var existingAllocation = await _context.CourseAllocations
                .FirstOrDefaultAsync(ca => ca.CourseId == courseAllocation.CourseId 
                    && ca.SectionId == courseAllocation.SectionId
                    && ca.SemesterId == courseAllocation.SemesterId
                    && ca.BatchId == courseAllocation.BatchId
                    && ca.Id != courseAllocation.Id);

            if (existingAllocation != null)
            {
                ModelState.AddModelError("", "This course section is already assigned to another teacher. Each section of a course can only be assigned to one teacher.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(courseAllocation);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Course allocation updated successfully!" });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseAllocationExists(courseAllocation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            ViewData["BatchId"] = new SelectList(_context.Batches, "Id", "Name", courseAllocation.BatchId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", courseAllocation.CourseId);
            ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Name", courseAllocation.SectionId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", courseAllocation.SemesterId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name", courseAllocation.TeacherId);
            return View(courseAllocation);
        }

        // GET: CourseAllocations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseAllocation = await _context.CourseAllocations
                .Include(c => c.Batch)
                .Include(c => c.Course)
                .Include(c => c.Section)
                .Include(c => c.Semester)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseAllocation == null)
            {
                return NotFound();
            }

            return View(courseAllocation);
        }

        // POST: CourseAllocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseAllocation = await _context.CourseAllocations.FindAsync(id);
            if (courseAllocation != null)
            {
                _context.CourseAllocations.Remove(courseAllocation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: CourseAllocations/DeleteAjax - AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var courseAllocation = await _context.CourseAllocations.FindAsync(id);
                if (courseAllocation == null)
                {
                    return Json(new { success = false, message = "Course allocation not found!" });
                }

                // Check for related enrollments
                var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseAllocationId == id);
                if (hasEnrollments)
                {
                    return Json(new { success = false, message = "Cannot delete allocation with existing student enrollments." });
                }

                // Check for related attendance records
                var hasAttendance = await _context.Attendances.AnyAsync(a => a.CourseAllocationId == id);
                if (hasAttendance)
                {
                    return Json(new { success = false, message = "Cannot delete allocation with existing attendance records." });
                }

                _context.CourseAllocations.Remove(courseAllocation);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Course allocation deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting allocation: " + ex.Message });
            }
        }

        private bool CourseAllocationExists(int id)
        {
            return _context.CourseAllocations.Any(e => e.Id == id);
        }

        // GET: Upload Page
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Process the Excel File
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ScheduleUploadViewModel model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                // Set License Context (Required for EPPlus 5+)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await model.File.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Read first sheet
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        // Loop through rows (Assuming Row 1 is Header, start from 2)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            // EXPECTED COLUMNS: 
                            // A: Teacher Email | B: Course Code | C: Batch Name | D: Section Name | E: Semester Name

                            var teacherEmail = worksheet.Cells[row, 1].Value?.ToString().Trim();
                            var courseCode = worksheet.Cells[row, 2].Value?.ToString().Trim();
                            var batchName = worksheet.Cells[row, 3].Value?.ToString().Trim();
                            var sectionName = worksheet.Cells[row, 4].Value?.ToString().Trim();
                            var semesterName = worksheet.Cells[row, 5].Value?.ToString().Trim();

                            if (string.IsNullOrEmpty(courseCode)) continue;

                            // 1. Find IDs from Database based on names
                            var teacherUser = !string.IsNullOrEmpty(teacherEmail) ? await _userManager.FindByEmailAsync(teacherEmail) : null;
                            var teacher = teacherUser != null ? _context.Teachers.FirstOrDefault(t => t.UserId == teacherUser.Id) : null;

                            var course = _context.Courses.FirstOrDefault(c => c.Code == courseCode);
                            var batch = _context.Batches.FirstOrDefault(b => b.Name == batchName);
                            var section = _context.Sections.FirstOrDefault(s => s.Name == sectionName);
                            var semester = _context.Semesters.FirstOrDefault(s => s.Name == semesterName);

                            // 2. Only Create if ALL parts exist
                            if (teacher != null && course != null && batch != null && section != null && semester != null)
                            {
                                // 3. Prevent Duplicates
                                bool exists = _context.CourseAllocations.Any(ca =>
                                    ca.TeacherId == teacher.Id &&
                                    ca.CourseId == course.Id &&
                                    ca.BatchId == batch.Id &&
                                    ca.SectionId == section.Id &&
                                    ca.SemesterId == semester.Id);

                                if (!exists)
                                {
                                    var allocation = new CourseAllocation
                                    {
                                        TeacherId = teacher.Id,
                                        CourseId = course.Id,
                                        BatchId = batch.Id,
                                        SectionId = section.Id,
                                        SemesterId = semester.Id
                                    };
                                    _context.CourseAllocations.Add(allocation);
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View();
        }
    }
}
