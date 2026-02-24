using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agrojob.UoW;

namespace Agrojob.Pages.EmployeeManagement.ResumeManagement
{
    [Authorize]
    public class MyResumesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public MyResumesModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 9;
        public int TotalPages { get; set; }

        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int ApplicationsCount { get; set; }

        public List<MyResumeViewModel> Resumes { get; set; } = new();
        public int CurrentPage => Page;

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadResumesAsync(userId);
        }

        private async Task LoadResumesAsync(string userId)
        {
            // Получаем все резюме пользователя
            var userResumes = await _unitOfWork.Resumes.GetResumesByUserAsync(userId);
            var resumesList = userResumes.ToList();

            // Считаем статистику
            TotalCount = resumesList.Count;
            ActiveCount = resumesList.Count(r => r.IsActive && r.IsPublished);
            ApplicationsCount = await _unitOfWork.Applications.GetApplicationsCountByUserAsync(userId);

            // Пагинация
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            var pageResumes = resumesList
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Маппим в ViewModel
            Resumes = pageResumes.Select(r => new MyResumeViewModel
            {
                Id = r.Id,
                Title = r.Title,
                FullName = r.FullName,
                DesiredSalary = r.DesiredSalary,
                ExperienceYears = r.ExperienceYears,
                Location = r.Location,
                ReadyToRelocate = r.ReadyToRelocate,
                ReadyForBusinessTrips = r.ReadyForBusinessTrips,
                IsActive = r.IsActive,
                IsPublished = r.IsPublished,
                CreatedAt = FormatPostedDate(r.CreatedAt)
            }).ToList();
        }

        private string FormatPostedDate(DateTime date)
        {
            var diff = DateTime.UtcNow - date;

            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} мин. назад";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} ч. назад";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} дн. назад";
            else
                return date.ToString("dd.MM.yyyy");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
            if (resume == null || resume.UserId != userId)
            {
                return NotFound();
            }

            await _unitOfWork.Resumes.DeleteAsync(id);
            TempData["SuccessMessage"] = "Резюме успешно удалено";

            return RedirectToPage();
        }
    }

    public class MyResumeViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DesiredSalary { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Location { get; set; }
        public bool ReadyToRelocate { get; set; }
        public bool ReadyForBusinessTrips { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}