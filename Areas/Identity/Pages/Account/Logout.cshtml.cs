// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AttendanceManagementSystem.Services;

namespace AttendanceManagementSystem.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IJwtService _jwtService;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, IJwtService jwtService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Invalidate JWT Token
            if (Request.Cookies.TryGetValue("JwtToken", out var jwtToken))
            {
                _jwtService.InvalidateToken(jwtToken);
                _logger.LogInformation("JWT Token invalidated.");
            }
            
            // Delete the JWT cookie
            Response.Cookies.Delete("JwtToken");
            
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
