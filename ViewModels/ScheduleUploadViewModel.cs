using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.ViewModels
{
    public class ScheduleUploadViewModel
    {
        [Required]
        [Display(Name = "Upload Excel File")]
        public IFormFile File { get; set; }
    }
}