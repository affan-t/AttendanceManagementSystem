using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.ViewModels;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AttendanceManagementSystemContext _context;

        public StudentsController(UserManager<IdentityUser> userManager, AttendanceManagementSystemContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Students
        // Lists all students and includes their Email from the Identity User table
        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Batch)
                .ToListAsync();
            return View(students);
        }

        // GET: Students/GetTableData - AJAX
        // Returns only the table rows for refreshing without full page reload
        public async Task<IActionResult> GetTableData()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Batch)
                .ToListAsync();
            return PartialView("_StudentsTable", students);
        }

        // GET: Students - AJAX
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Batch)
                .Select(s => new {
                    s.Id,
                    s.Name,
                    s.RollNo,
                    s.Department,
                    BatchName = s.Batch != null ? s.Batch.Name : "Not Assigned",
                    Email = s.User != null ? s.User.Email : ""
                })
                .ToListAsync();
            return Json(students);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            // Fetch batches to show in the dropdown list
            ViewData["BatchId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Batches, "Id", "Name");
            
            // Return partial view for modal
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateModal");
            }
            
            return View();
        }

        // Post: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");

                    var student = new Student
                    {
                        UserId = user.Id,
                        Name = model.Name,
                        FatherName = model.FatherName,
                        CNIC = model.CNIC,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        Department = model.Department,
                        Degree = model.Degree,
                        EnrollmentYear = model.EnrollmentYear,
                        RollNo = model.RollNo,
                        IntakeBatch = model.IntakeBatch,
                        BatchId = model.BatchId // Assign batch/session
                    };

                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Student created successfully!" });
                    }
                    return RedirectToAction(nameof(Index));
                }
                
                var errors = result.Errors.Select(e => e.Description).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            // Reload Batch/Session Dropdown on error
            ViewData["BatchId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Batches, "Id", "Name", model.BatchId);
            return View(model);
        }

        // GET: Students/Edit/
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // Fetch the Login Info (Email)
            var user = await _userManager.FindByIdAsync(student.UserId);

            // Populate the ViewModel
            var model = new StudentEditViewModel
            {
                Id = student.Id,
                Email = user?.Email ?? "", // Load existing email
                Name = student.Name,
                FatherName = student.FatherName,
                CNIC = student.CNIC,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                Department = student.Department,
                Degree = student.Degree,
                EnrollmentYear = student.EnrollmentYear,
                RollNo = student.RollNo,
                IntakeBatch = student.IntakeBatch,
                BatchId = student.BatchId
            };

            // Load batch dropdown
            ViewData["BatchId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Batches, "Id", "Name", student.BatchId);
            
            // Return partial view for modal
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditModal", model);
            }
            
            return View(model);
        }

        // POST: Students/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Update Student Profile (Local DB)
                    var student = await _context.Students.FindAsync(id);
                    if (student == null) return NotFound();

                    student.Name = model.Name;
                    student.FatherName = model.FatherName;
                    student.CNIC = model.CNIC;
                    student.PhoneNumber = model.PhoneNumber;
                    student.Address = model.Address;
                    student.Department = model.Department;
                    student.Degree = model.Degree;
                    student.EnrollmentYear = model.EnrollmentYear;
                    student.RollNo = model.RollNo;
                    student.IntakeBatch = model.IntakeBatch;
                    student.BatchId = model.BatchId;

                    _context.Update(student);
                    await _context.SaveChangesAsync();

                    // 2. Update Login Details (Identity DB)
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    if (user != null)
                    {
                        // A. Update Email/Username if changed
                        if (user.Email != model.Email)
                        {
                            user.Email = model.Email;
                            user.UserName = model.Email;
                            await _userManager.UpdateAsync(user);
                        }

                        // B. Update Password ONLY if the field is not empty
                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            // Force reset the password without needing the old one
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                            if (!result.Succeeded)
                            {
                                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                                {
                                    var errors = result.Errors.Select(e => e.Description).ToList();
                                    return Json(new { success = false, errors });
                                }
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError("Password", error.Description);
                                }
                                return View(model);
                            }
                        }
                    }

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Student updated successfully!" });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            // Reload batch dropdown on error
            ViewData["BatchId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Batches, "Id", "Name", model.BatchId);
            return View(model);
        }

        // POST: Students/Delete/5 - AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found!" });
                }

                // 1. Find the Login Account (Identity User)
                var user = await _userManager.FindByIdAsync(student.UserId);

                // 2. Remove Related Data First (Enrollments & Attendance)
                var enrollments = _context.Enrollments.Where(e => e.StudentId == id);
                _context.Enrollments.RemoveRange(enrollments);

                var attendances = _context.Attendances.Where(a => a.StudentId == id);
                _context.Attendances.RemoveRange(attendances);

                // 3. Now Delete the Student Profile
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // 4. Finally, Delete the Login Account
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return Json(new { success = true, message = "Student deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting student: " + ex.Message });
            }
        }

        // POST: Students/Delete/5 - Traditional
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // 1. Find the Login Account (Identity User)
                var user = await _userManager.FindByIdAsync(student.UserId);

                // 2. Remove Related Data First (Enrollments & Attendance)
                var enrollments = _context.Enrollments.Where(e => e.StudentId == id);
                _context.Enrollments.RemoveRange(enrollments);

                var attendances = _context.Attendances.Where(a => a.StudentId == id);
                _context.Attendances.RemoveRange(attendances);

                // 3. Now Delete the Student Profile
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // 4. Finally, Delete the Login Account
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}