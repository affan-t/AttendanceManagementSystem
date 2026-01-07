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
    public class TeachersController : Controller
    {
        private readonly AttendanceManagementSystemContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // === CONSTRUCTOR: INJECTS BOTH DB CONTEXT AND USER MANAGER ===
        public TeachersController(AttendanceManagementSystemContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Teachers
        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            return View(teachers);
        }

        // GET: Teachers/GetTableData - AJAX
        public async Task<IActionResult> GetTableData()
        {
            var teachers = await _context.Teachers.Include(t => t.User).ToListAsync();
            return PartialView("_TeachersTable", teachers);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateModal");
            }
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Create Login
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");

                    // 2. Create Profile
                    var teacher = new Teacher
                    {
                        UserId = user.Id,
                        Name = model.Name,
                        FatherName = model.FatherName,
                        CNIC = model.CNIC,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        Designation = model.Designation,
                        Department = model.Department
                    };

                    _context.Add(teacher);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Teacher created successfully!" });
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
            return View(model);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            var user = await _userManager.FindByIdAsync(teacher.UserId);

            var model = new TeacherEditViewModel
            {
                Id = teacher.Id,
                Email = user?.Email ?? "",
                Name = teacher.Name,
                FatherName = teacher.FatherName,
                CNIC = teacher.CNIC,
                PhoneNumber = teacher.PhoneNumber,
                Address = teacher.Address,
                Designation = teacher.Designation,
                Department = teacher.Department
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditModal", model);
            }

            return View(model);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var teacher = await _context.Teachers.FindAsync(id);
                    if (teacher == null) return NotFound();

                    // 1. Update Profile
                    teacher.Name = model.Name;
                    teacher.FatherName = model.FatherName;
                    teacher.CNIC = model.CNIC;
                    teacher.PhoneNumber = model.PhoneNumber;
                    teacher.Address = model.Address;
                    teacher.Designation = model.Designation;
                    teacher.Department = model.Department;

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();

                    // 2. Update Login
                    var user = await _userManager.FindByIdAsync(teacher.UserId);
                    if (user != null)
                    {
                        if (user.Email != model.Email)
                        {
                            user.Email = model.Email;
                            user.UserName = model.Email;
                            await _userManager.UpdateAsync(user);
                        }

                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                            if (!result.Succeeded)
                            {
                                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                                {
                                    var errors = result.Errors.Select(e => e.Description).ToList();
                                    return Json(new { success = false, errors });
                                }
                                foreach (var error in result.Errors) ModelState.AddModelError("Password", error.Description);
                                return View(model);
                            }
                        }
                    }

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Teacher updated successfully!" });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teachers.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }
            return View(model);
        }

        // POST: Teachers/DeleteAjax - AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                {
                    return Json(new { success = false, message = "Teacher not found!" });
                }

                // Check for related course allocations
                var hasAllocations = await _context.CourseAllocations.AnyAsync(ca => ca.TeacherId == id);
                if (hasAllocations)
                {
                    return Json(new { success = false, message = "Cannot delete teacher with assigned courses. Please remove course allocations first." });
                }

                var user = await _userManager.FindByIdAsync(teacher.UserId);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return Json(new { success = true, message = "Teacher deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting teacher: " + ex.Message });
            }
        }

        // POST: Teachers/Delete/5 - Traditional
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                var user = await _userManager.FindByIdAsync(teacher.UserId);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}