using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agrojob.UoW;
using Agrojob.Models;

namespace Agrojob.Pages
{
    [Authorize]
    public class EmployeeManagementMenuModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeManagementMenuModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public int ResumesCount { get; set; }
        public int ApplicationsCount { get; set; }
        public int FavoritesCount { get; set; }
        public int TotalVacancies { get; set; }
        public int RecommendedCount { get; set; }
        public List<RecentVacancyViewModel> RecentVacancies { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadCountersAsync(userId);
            await LoadRecentVacanciesAsync(userId);
        }

        private async Task LoadCountersAsync(string userId)
        {
            // Количество резюме пользователя
            var resumes = await _unitOfWork.Resumes.GetResumesByUserAsync(userId);
            ResumesCount = resumes.Count();

            // Количество откликов пользователя
            ApplicationsCount = await _unitOfWork.Applications.GetApplicationsCountByUserAsync(userId);

            // Общее количество активных вакансий в системе
            var allVacancies = await _unitOfWork.Vacancies.FindAsync(v => v.IsActive);
            TotalVacancies = allVacancies.Count();

            // Рекомендации (заглушка, потом можно сделать алгоритм)
            RecommendedCount = 12;

            // Избранное (заглушка, если нет функционала избранного)
            FavoritesCount = 3;
        }

        private async Task LoadRecentVacanciesAsync(string userId)
        {
            // Заглушка для недавно просмотренных
            // В реальности нужно брать из истории просмотров
            var recent = await _unitOfWork.Vacancies.GetLatestVacanciesAsync(3);

            RecentVacancies = recent.Select(v => new RecentVacancyViewModel
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company?.Name ?? "Не указано",
                Salary = v.Salary ?? "Не указана"
            }).ToList();
        }
    }

    public class RecentVacancyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
    }
}