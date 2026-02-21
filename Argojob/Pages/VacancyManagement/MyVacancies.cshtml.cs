using System.Security.Claims;
using Agrojob.Models;
using Agrojob.Repositories;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Agrojob.Pages.VacancyManagement
{
    [Authorize]
    public class MyVacanciesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public MyVacanciesModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int TotalViews { get; set; }

        public List<MyVacancyViewModel> Vacancies { get; set; } = new();
        public int CurrentPage => Page;

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadVacanciesAsync(userId);
        }

        private async Task LoadVacanciesAsync(string userId)
        {
            // Получаем все вакансии пользователя
            var allVacancies = await _unitOfWork.Vacancies
                .FindAsync(v => v.CreatedById == userId);

            var vacanciesList = allVacancies.ToList();

            // Считаем статистику
            TotalCount = vacanciesList.Count;
            ActiveCount = vacanciesList.Count(v => v.IsActive);
            InactiveCount = TotalCount - ActiveCount;
            TotalViews = vacanciesList.Sum(v => v.ViewsCount);

            // Пагинация
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            var pageVacancies = vacanciesList
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Маппим в ViewModel
            Vacancies = pageVacancies.Select(v => new MyVacancyViewModel
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company?.Name ?? "Не указано",
                Location = v.Location?.Name ?? "Не указано",
                Salary = v.Salary ?? "Не указана",
                PostedDate = FormatPostedDate(v.PostedDate),
                IsSeasonal = v.IsSeasonal,
                IsActive = v.IsActive,
                ViewsCount = v.ViewsCount
            }).ToList();
        }

        private string FormatPostedDate(DateTime postedDate)
        {
            var diff = DateTime.UtcNow - postedDate;

            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} мин. назад";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} ч. назад";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} дн. назад";
            else
                return postedDate.ToString("dd.MM.yyyy");
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);

            if (vacancy == null || vacancy.CreatedById != userId)
            {
                return NotFound();
            }

            vacancy.IsActive = !vacancy.IsActive;
            await _unitOfWork.Vacancies.UpdateAsync(vacancy);

            TempData["SuccessMessage"] = vacancy.IsActive
                ? "Вакансия активирована"
                : "Вакансия деактивирована";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);

            if (vacancy == null || vacancy.CreatedById != userId)
            {
                return NotFound();
            }

            await _unitOfWork.Vacancies.DeleteAsync(id);
            TempData["SuccessMessage"] = "Вакансия удалена";

            return RedirectToPage();
        }
    }

    public class MyVacancyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public string PostedDate { get; set; } = string.Empty;
        public bool IsSeasonal { get; set; }
        public bool IsActive { get; set; }
        public int ViewsCount { get; set; }
    }
}