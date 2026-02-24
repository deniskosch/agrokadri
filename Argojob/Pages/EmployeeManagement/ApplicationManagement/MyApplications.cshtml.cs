using System.Security.Claims;
using Agrojob.Data;
using Agrojob.Models;
using Agrojob.UoW;
using Agrojob.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Agrojob.Pages.EmployeeManagement.ApplicationManagement
{
    [Authorize]
    public class MyApplicationsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyApplicationsModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [FromQuery]
        public string? Status { get; set; }

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int CurrentPage => Page;
        public string? CurrentStatus => Status;

        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ViewedCount { get; set; }
        public int InvitedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }

        public List<MyApplicationViewModel> Applications { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await LoadApplicationsAsync(userId);
            await LoadStatisticsAsync(userId);

            return Page();
        }

        private async Task LoadApplicationsAsync(string userId)
        {
            var allApplications = await _unitOfWork.Applications.GetApplicationsByUserAsync(userId);

            // Фильтрация по статусу
            var filtered = allApplications;
            if (!string.IsNullOrEmpty(Status))
            {
                var statusEnum = Status switch
                {
                    "pending" => ApplicationStatus.Pending,
                    "viewed" => ApplicationStatus.Viewed,
                    "invited" => ApplicationStatus.Invited,
                    "accepted" => ApplicationStatus.Accepted,
                    "rejected" => ApplicationStatus.Rejected,
                    "withdrawn" => ApplicationStatus.Withdrawn,
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

            // Маппинг в ViewModel
            foreach (var app in pageApplications)
            {
                var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(app.VacancyId);

                Applications.Add(new MyApplicationViewModel
                {
                    Id = app.Id,
                    VacancyId = app.VacancyId,
                    VacancyTitle = vacancy?.Title ?? "Вакансия удалена",
                    CompanyName = vacancy?.Company?.Name ?? "Не указано",
                    Location = vacancy?.Location?.Name ?? "Не указано",
                    Salary = vacancy?.Salary ?? "Не указана",
                    Status = app.Status,
                    StatusDisplay = app.Status.GetDisplayName(),
                    StatusColor = app.Status.GetColorClass(),
                    AppliedAt = app.AppliedAt,
                    AppliedAtDisplay = FormatDate(app.AppliedAt),
                    StatusUpdatedAt = app.StatusUpdatedAt,
                    StatusUpdatedAtDisplay = app.StatusUpdatedAt.HasValue ? FormatDate(app.StatusUpdatedAt.Value) : null,
                    CoverLetter = app.CoverLetter,
                    EmployerComment = app.EmployerComment,
                    CanWithdraw = app.Status == ApplicationStatus.Pending || app.Status == ApplicationStatus.Viewed
                });
            }
        }

        private async Task LoadStatisticsAsync(string userId)
        {
            var allApps = await _unitOfWork.Applications.GetApplicationsByUserAsync(userId);

            PendingCount = allApps.Count(a => a.Status == ApplicationStatus.Pending);
            ViewedCount = allApps.Count(a => a.Status == ApplicationStatus.Viewed);
            InvitedCount = allApps.Count(a => a.Status == ApplicationStatus.Invited);
            AcceptedCount = allApps.Count(a => a.Status == ApplicationStatus.Accepted);
            RejectedCount = allApps.Count(a => a.Status == ApplicationStatus.Rejected);
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

        public async Task<IActionResult> OnPostWithdrawAsync(int applicationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);
            if (application == null || application.UserId != userId)
            {
                return NotFound();
            }

            // Можно отменить только отклики в статусе Pending или Viewed
            if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.Viewed)
            {
                TempData["ErrorMessage"] = "Нельзя отменить этот отклик";
                return RedirectToPage();
            }

            await _unitOfWork.Applications.UpdateStatusAsync(applicationId, ApplicationStatus.Withdrawn, "Отменено соискателем");

            TempData["SuccessMessage"] = "Отклик успешно отменен";
            return RedirectToPage();
        }
    }

    public class MyApplicationViewModel
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string AppliedAtDisplay { get; set; } = string.Empty;
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedAtDisplay { get; set; }
        public string? CoverLetter { get; set; }
        public string? EmployerComment { get; set; }
        public bool CanWithdraw { get; set; }
    }
}
