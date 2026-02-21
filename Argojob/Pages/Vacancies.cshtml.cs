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

            // По категориям
            AgronomCount = await _unitOfWork.Vacancies.CountAsync(v =>
                v.IsActive && v.Category != null && v.Category.Name == "Агроном");

            VeterinarCount = await _unitOfWork.Vacancies.CountAsync(v =>
                v.IsActive && v.Category != null && v.Category.Name == "Ветеринар");

            MechanizatorCount = await _unitOfWork.Vacancies.CountAsync(v =>
                v.IsActive && v.Category != null && v.Category.Name == "Механизатор");

            TechnologCount = await _unitOfWork.Vacancies.CountAsync(v =>
                v.IsActive && v.Category != null && v.Category.Name == "Технолог");

            EngineerCount = await _unitOfWork.Vacancies.CountAsync(v =>
                v.IsActive && v.Category != null && v.Category.Name == "Инженер");
        }

        private async Task LoadVacanciesAsync()
        {
            // Определяем ID категории если выбрана не "all"
            int? categoryId = null;
            if (Category != "all" && Category != "сезонные")
            {
                var category = await _unitOfWork.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == Category.ToLower());
                categoryId = category?.Id;
            }

            // Определяем фильтр по сезонности
            bool? isSeasonal = Type switch
            {
                "seasonal" => true,
                "permanent" => false,
                _ => null
            };

            // Получаем все ID вакансий по фильтру для подсчета общего количества
            var allVacancyIds = await _unitOfWork.Vacancies
                .FindAsync(v => v.IsActive &&
                    (categoryId == null || v.CategoryId == categoryId) &&
                    (Category != "сезонные" || v.IsSeasonal) &&
                    (isSeasonal == null || v.IsSeasonal == isSeasonal) &&
                    (string.IsNullOrEmpty(Search) ||
                     v.Title.Contains(Search) ||
                     v.Description.Contains(Search) ||
                     (v.Company != null && v.Company.Name.Contains(Search))))
                .ContinueWith(t => t.Result.Select(v => v.Id).ToList());

            var totalCount = allVacancyIds.Count;
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)PageSize);

            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            // Получаем ID для текущей страницы
            var pageIds = allVacancyIds
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Загружаем вакансии с деталями
            var vacancies = new List<VacancyViewModel>();
            foreach (var id in pageIds)
            {
                var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(id);
                if (vacancy != null)
                {
                    vacancies.Add(new VacancyViewModel
                    {
                        Id = vacancy.Id,
                        Title = vacancy.Title,
                        Company = vacancy.Company?.Name ?? "Не указано",
                        Location = vacancy.Location?.Name ?? "Не указано",
                        Salary = vacancy.Salary,
                        Tags = vacancy.VacancyTags?.Select(vt => vt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new(),
                        Category = vacancy.Category?.Name ?? "",
                        PostedDate = FormatPostedDate(vacancy.PostedDate),
                        IsSeasonal = vacancy.IsSeasonal,
                        Description = vacancy.Description
                    });
                }
            }

            FilteredVacancies = vacancies;
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