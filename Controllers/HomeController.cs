using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks; // Added this

namespace AttendanceManagementSystem.Controllers // Make sure this namespace matches
{
    public class HomeController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        // 1. Added constructor to get Identity services
        public HomeController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 2. Modified Index to be a smart redirector
        public async Task<IActionResult> Index()
        {
            // If the user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Teacher"))
                    {
                        return RedirectToAction("Index", "Teacher");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Student"))
                    {
                        return RedirectToAction("Index", "Student");
                    }
                }
            }

            // If user is NOT logged in, send them to the Login page
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // You can leave the Privacy action or remove it
        public IActionResult Privacy()
        {
            return View();
        }

        // Error action is still useful
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // ... (this method remains unchanged) ...
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}