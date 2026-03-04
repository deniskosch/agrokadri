using System.Security.Claims;
using Agrojob.Models;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Agrojob.Pages.EmployeeManagement.ResumeManagement
{
    [Authorize(Roles = "Employee")]
    public class ResumePreviewModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ResumePreviewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Resume Resume { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Загружаем резюме с деталями
            Resume = await _unitOfWork.Resumes.GetResumeWithDetailsAsync(id);

            if (Resume == null)
            {
                return NotFound();
            }

            // Проверяем, принадлежит ли резюме текущему пользователю
            if (Resume.UserId != userId)
            {
                return Forbid();
            }

            return Page();
        }
    }
}