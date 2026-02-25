using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Agrojob.Models;
using Agrojob.Repositories;
using Agrojob.UoW;
using Agrojob.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

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

        // Для модалки отклика
        public List<Resume> UserResumes { get; set; } = new();
        public bool HasApplied { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(id);

            if (vacancy == null || !vacancy.IsActive)
            {
                return NotFound();
            }

            Vacancy = MapToViewModel(vacancy);

            // Если пользователь авторизован, загружаем его резюме и проверяем отклик
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var resumes = await _unitOfWork.Resumes.GetResumesByUserAsync(userId);
                    UserResumes = resumes.Where(r => r.IsActive).ToList();

                    HasApplied = await _unitOfWork.Applications.HasUserAppliedToVacancyAsync(userId, id);
                }
            }

            return Page();
        }

        private VacancyDetailViewModel MapToViewModel(Vacancy vacancy)
        {
            return new VacancyDetailViewModel
            {
                Id = vacancy.Id,
                Title = vacancy.Title,
                Company = vacancy.Company?.Name ?? "Не указано",
                Location = vacancy.Location ?? "Не указано",
                Salary = vacancy.Salary ?? "Не указана",
                Tags = vacancy.VacancyTags?
                    .Select(vt => vt.Tag?.Name ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList() ?? new(),
                Category = vacancy.Category ?? "",
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

        [Authorize]
        public async Task<IActionResult> OnPostApplyAsync(int vacancyId, int resumeId, string? coverLetter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Проверяем, не откликался ли уже
            var hasApplied = await _unitOfWork.Applications.HasUserAppliedToVacancyAsync(userId, vacancyId);
            if (hasApplied)
            {
                TempData["ErrorMessage"] = "Вы уже откликались на эту вакансию";
                return RedirectToPage(new { id = vacancyId });
            }

            // Проверяем, существует ли резюме и принадлежит ли пользователю
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            if (resume == null || resume.UserId != userId)
            {
                TempData["ErrorMessage"] = "Резюме не найдено";
                return RedirectToPage(new { id = vacancyId });
            }

            // Проверяем, активна ли вакансия
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(vacancyId);
            if (vacancy == null || !vacancy.IsActive)
            {
                TempData["ErrorMessage"] = "Вакансия не активна";
                return RedirectToPage(new { id = vacancyId });
            }

            // Создаем отклик
            var application = new Application
            {
                VacancyId = vacancyId,
                UserId = userId,
                ResumeId = resumeId,
                CoverLetter = coverLetter,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.Applications.AddAsync(application);

            TempData["SuccessMessage"] = "Отклик успешно отправлен!";
            return RedirectToPage(new { id = vacancyId });
        }
    }
}