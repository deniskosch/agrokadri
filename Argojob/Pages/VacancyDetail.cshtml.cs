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
    public class VacancyDetailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public VacancyDetailModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromRoute]
        public int Id { get; set; }

        public VacancyDetailViewModel? Vacancy { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(id);

            if (vacancy == null || !vacancy.IsActive)
            {
                return NotFound();
            }

            Vacancy = MapToViewModel(vacancy);

            return Page();
        }

        private VacancyDetailViewModel MapToViewModel(Vacancy vacancy)
        {
            return new VacancyDetailViewModel
            {
                Id = vacancy.Id,
                Title = vacancy.Title,
                Company = vacancy.Company?.Name ?? "Не указано",
                Location = vacancy.Location?.Name ?? "Не указано",
                Salary = vacancy.Salary ?? "Не указана",
                Tags = vacancy.VacancyTags?
                    .Select(vt => vt.Tag?.Name ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList() ?? new(),
                Category = vacancy.Category?.Name ?? "",
                PostedDate = FormatPostedDate(vacancy.PostedDate),
                IsSeasonal = vacancy.IsSeasonal,
                Description = vacancy.Description,
                Requirements = vacancy.Requirements?
                    .Select(r => r.Text)
                    .ToList() ?? new(),
                Offers = vacancy.Offers?
                    .Select(o => o.Text)
                    .ToList() ?? new(),
                ContactPerson = vacancy.Company?.ContactPerson,
                ContactPhone = vacancy.Company?.ContactPhone,
                ContactEmail = vacancy.Company?.ContactEmail
            };
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
    }
}