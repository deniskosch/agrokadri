using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agrojob.Models;
using Agrojob.Repositories;
using Agrojob.UoW;
using Agrojob.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Agrojob.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Свойства для страницы
        public string CurrentFilter { get; set; } = "all";
        public string SearchTerm { get; set; } = string.Empty;
        public int VisibleCount { get; set; } = 6;
        public List<VacancyViewModel> DisplayedVacancies { get; set; } = new();
        public bool HasMoreVacancies { get; set; }

        public async Task OnGetAsync(string filter = "all", string search = null)
        {
            CurrentFilter = filter ?? "all";
            SearchTerm = search ?? string.Empty;

            await LoadVacanciesAsync();
        }

        private async Task LoadVacanciesAsync()
        {
            // Получаем все активные вакансии
            var allVacancies = await _unitOfWork.Vacancies.GetAllAsync();
            var query = allVacancies.Where(v => v.IsActive).AsEnumerable();

            // Применяем фильтр по категории
            if (CurrentFilter != "all")
            {
                if (CurrentFilter == "сезонные")
                {
                    query = query.Where(v => v.IsSeasonal);
                }
                else
                {
                    // Фильтр по строковой категории
                    query = query.Where(v => !string.IsNullOrEmpty(v.Category) &&
                        v.Category.ToLower() == CurrentFilter.ToLower());
                }
            }

            // Применяем поиск
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(searchLower) ||
                    (v.Company != null && v.Company.Name.ToLower().Contains(searchLower)) ||
                    v.Description.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(v.Category) && v.Category.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(v.Location) && v.Location.ToLower().Contains(searchLower)) ||
                    v.VacancyTags.Any(vt => vt.Tag != null &&
                        vt.Tag.Name.ToLower().Contains(searchLower)));
            }

            var filteredList = query.ToList();

            // Проверяем, есть ли еще вакансии для загрузки
            HasMoreVacancies = filteredList.Count > VisibleCount;

            // Берем только нужное количество
            var displayedVacancies = filteredList.Take(VisibleCount).ToList();

            // Маппим в ViewModel
            DisplayedVacancies = displayedVacancies.Select(v => new VacancyViewModel
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company?.Name ?? "Не указано",
                Location = v.Location ?? "Не указано", // Теперь строка
                Salary = v.Salary ?? "Не указана",
                Tags = v.VacancyTags?
                    .Where(vt => vt.Tag != null)
                    .Select(vt => vt.Tag!.Name)
                    .ToList() ?? new(),
                Category = v.Category ?? "", // Теперь строка
                PostedDate = FormatPostedDate(v.PostedDate),
                IsSeasonal = v.IsSeasonal
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

        public async Task<IActionResult> OnPostLoadMoreAsync(string currentFilter, int currentVisibleCount)
        {
            CurrentFilter = currentFilter;
            VisibleCount = currentVisibleCount + 3;

            await LoadVacanciesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDetailsAsync(int id)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
            if (vacancy != null)
            {
                TempData["Message"] = $"Открыта вакансия: {vacancy.Title}\nКомпания: {vacancy.Company?.Name}";
            }
            return RedirectToPage("VacancyDetail", new { id });
        }
    }
}