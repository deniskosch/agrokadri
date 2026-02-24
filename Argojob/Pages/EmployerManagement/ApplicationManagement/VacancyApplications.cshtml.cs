using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Agrojob.UoW;
using Agrojob.Data;
using Agrojob.ViewModels;
using Agrojob.Models;

namespace Agrojob.Pages.EmployerManagement.ApplicationManagement
{
    [Authorize]
    public class VacancyApplicationsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public VacancyApplicationsModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [FromRoute]
        public int VacancyId { get; set; }

        [FromQuery]
        public string? Status { get; set; }

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int CurrentPage => Page;
        public string? CurrentStatus => Status;

        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ViewedCount { get; set; }
        public int AcceptedCount { get; set; }

        public List<string> AvailableStatuses { get; set; } = new()
        {
            "На рассмотрении", "Просмотрено", "Приглашение на собеседование", "Принят", "Отказ"
        };

        public List<ApplicationListItemViewModel> Applications { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int vacancyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Проверяем, имеет ли пользователь доступ к вакансии
            var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(vacancyId);
            if (vacancy == null)
            {
                return NotFound();
            }

            // Проверяем, является ли пользователь владельцем вакансии или имеет доступ к компании
            var isOwner = vacancy.CreatedById == userId;
            var hasCompanyAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, vacancy.CompanyId);

            if (!isOwner && !hasCompanyAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            VacancyTitle = vacancy.Title;
            CompanyName = vacancy.Company?.Name ?? "Не указано";

            await LoadApplicationsAsync(vacancyId);
            await LoadStatisticsAsync(vacancyId);

            return Page();
        }

        private async Task LoadApplicationsAsync(int vacancyId)
        {
            var allApplications = await _unitOfWork.Applications.GetApplicationsByVacancyAsync(vacancyId);

            // Фильтрация по статусу
            var filtered = allApplications;
            if (!string.IsNullOrEmpty(Status))
            {
                var statusEnum = Status switch
                {
                    "На рассмотрении" => ApplicationStatus.Pending,
                    "Просмотрено" => ApplicationStatus.Viewed,
                    "Приглашение на собеседование" => ApplicationStatus.Invited,
                    "Принят" => ApplicationStatus.Accepted,
                    "Отказ" => ApplicationStatus.Rejected,
                    _ => (ApplicationStatus?)null
                };

                if (statusEnum.HasValue)
                {
                    filtered = filtered.Where(a => a.Status == statusEnum.Value);
                }
            }

            var applicationsList = filtered.ToList();
            TotalCount = applicationsList.Count;

            // Пагинация
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            var pageApplications = applicationsList
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Маппинг в ViewModel с использованием UserManager
            foreach (var app in pageApplications)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                var resume = app.ResumeId.HasValue
                    ? await _unitOfWork.Resumes.GetByIdAsync(app.ResumeId.Value)
                    : null;

                Applications.Add(new ApplicationListItemViewModel
                {
                    Id = app.Id,
                    VacancyId = vacancyId,
                    VacancyTitle = VacancyTitle,
                    ApplicantName = user?.FullName ?? app.UserId,
                    ApplicantEmail = user?.Email ?? "Не указан",
                    ApplicantPhone = user?.PhoneNumber,
                    ResumeTitle = resume?.Title,
                    ResumeId = resume?.Id,
                    Status = app.Status,
                    StatusDisplay = app.Status.GetDisplayName(),
                    StatusColor = app.Status.GetColorClass(),
                    AppliedAt = app.AppliedAt,
                    AppliedAtDisplay = FormatDate(app.AppliedAt),
                    CoverLetterPreview = app.CoverLetter?.Length > 100
                        ? app.CoverLetter.Substring(0, 100) + "..."
                        : app.CoverLetter
                });
            }
        }

        private async Task LoadStatisticsAsync(int vacancyId)
        {
            var allApps = await _unitOfWork.Applications.GetApplicationsByVacancyAsync(vacancyId);

            PendingCount = allApps.Count(a => a.Status == ApplicationStatus.Pending);
            ViewedCount = allApps.Count(a => a.Status == ApplicationStatus.Viewed);
            AcceptedCount = allApps.Count(a => a.Status == ApplicationStatus.Accepted);
        }

        private string FormatDate(DateTime date)
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
    }
}