using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agrojob.Models;
using Agrojob.Repositories;
using Agrojob.UoW;
using Agrojob.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Pages
{
    public class VacanciesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public VacanciesModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Параметры страницы
        [FromQuery]
        public string Category { get; set; } = "all";

        [FromQuery]
        public string Search { get; set; } = string.Empty;

        [FromQuery]
        public string Type { get; set; } = "all";

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 6;
        public int TotalPages { get; set; }
        public int TotalVacancies { get; set; }

        // Свойства для отображения
        public string CurrentCategory => Category;
        public string EmploymentType => Type;
        public int CurrentPage => Page;

        // Счетчики по категориям
        public int SeasonalCount { get; set; }
        public int AgronomCount { get; set; }
        public int VeterinarCount { get; set; }
        public int MechanizatorCount { get; set; }
        public int TechnologCount { get; set; }
        public int EngineerCount { get; set; }

        // Отфильтрованные вакансии
        public List<VacancyViewModel> FilteredVacancies { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadCountersAsync();
            await LoadVacanciesAsync();
        }

        private async Task LoadCountersAsync()
        {
            // Общее количество активных вакансий
            TotalVacancies = await _unitOfWork.Vacancies.CountAsync(v => v.IsActive);

            // Сезонные
            SeasonalCount = await _unitOfWork.Vacancies.CountAsync(v => v.IsActive && v.IsSeasonal);
        }

        private async Task LoadVacanciesAsync()
        {
            // Получаем все активные вакансии
            var allVacancies = await _unitOfWork.Vacancies.GetActiveVacanciesAsync();

            // Применяем фильтры в памяти (или можно через LINQ to Entities, если есть IQueryable)
            var query = allVacancies.AsEnumerable();

            // Фильтр по категории
            if (!string.IsNullOrEmpty(Category) && Category != "all")
            {
                if (Category == "сезонные")
                {
                    query = query.Where(v => v.IsSeasonal);
                }
                else
                {
                    // Поиск по строковой категории (регистронезависимый)
                    var categoryLower = Category.ToLower();
                    query = query.Where(v => !string.IsNullOrEmpty(v.Category) &&
                        v.Category.ToLower().Contains(categoryLower));
                }
            }

            // Фильтр по типу занятости
            if (!string.IsNullOrEmpty(Type) && Type != "all")
            {
                if (Type == "seasonal")
                {
                    query = query.Where(v => v.IsSeasonal);
                }
                else if (Type == "permanent")
                {
                    query = query.Where(v => !v.IsSeasonal);
                }
            }

            // Поиск по тексту
            if (!string.IsNullOrEmpty(Search))
            {
                var searchLower = Search.ToLower();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(searchLower) ||
                    v.Description.ToLower().Contains(searchLower) ||
                    (v.Company != null && v.Company.Name.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(v.Category) && v.Category.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(v.Location) && v.Location.ToLower().Contains(searchLower)) ||
                    v.VacancyTags.Any(vt => vt.Tag != null && vt.Tag.Name.ToLower().Contains(searchLower)));
            }

            var filteredList = query.ToList();
            var totalCount = filteredList.Count;

            // Пагинация
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)PageSize);

            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            // Берем вакансии для текущей страницы
            var pagedVacancies = filteredList
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Маппим в ViewModel
            FilteredVacancies = pagedVacancies.Select(v => new VacancyViewModel
            {
                Id = v.Id,
                Title = v.Title,
                Company = v.Company?.Name ?? "Не указано",
                Location = v.Location ?? "Не указано", // Теперь просто строка
                Salary = v.Salary ?? "Не указана",
                Tags = v.VacancyTags?
                    .Where(vt => vt.Tag != null)
                    .Select(vt => vt.Tag!.Name)
                    .ToList() ?? new(),
                Category = v.Category ?? "", // Теперь просто строка
                PostedDate = FormatPostedDate(v.PostedDate),
                IsSeasonal = v.IsSeasonal,
                Description = v.Description
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

        public string GetCategoryTitle()
        {
            return Category switch
            {
                "all" => "Все вакансии агропромышленного комплекса",
                "сезонные" => "Сезонные вакансии в сельском хозяйстве",
                "агроном" => "Вакансии для агрономов",
                "ветеринар" => "Вакансии для ветеринаров",
                "механизатор" => "Вакансии для механизаторов",
                "технолог" => "Вакансии для технологов",
                "инженер" => "Вакансии для инженеров",
                _ => "Вакансии в АПК"
            };
        }
    }

}