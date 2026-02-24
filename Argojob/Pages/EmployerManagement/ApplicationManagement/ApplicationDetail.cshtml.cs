using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Agrojob.UoW;
using Agrojob.Data;
using Agrojob.Models;
using Agrojob.ViewModels;

namespace Agrojob.Pages.EmployerManagement.ApplicationManagement
{
    [Authorize]
    public class ApplicationDetailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationDetailModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [FromRoute]
        public int Id { get; set; }

        public ApplicationDetailViewModel? Application { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            // Проверяем, имеет ли пользователь доступ к этому отклику
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(application.VacancyId);
            if (vacancy == null)
            {
                return NotFound();
            }

            var isOwner = vacancy.CreatedById == userId;
            var hasCompanyAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, vacancy.CompanyId);

            if (!isOwner && !hasCompanyAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Помечаем как просмотрено, если статус "На рассмотрении"
            if (application.Status == ApplicationStatus.Pending)
            {
                await _unitOfWork.Applications.UpdateStatusAsync(id, ApplicationStatus.Viewed);
                application.Status = ApplicationStatus.Viewed;
            }

            await LoadApplicationAsync(application);

            return Page();
        }

        private async Task LoadApplicationAsync(Application application)
        {
            var user = await _userManager.FindByIdAsync(application.UserId);
            var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(application.VacancyId);
            var resume = application.ResumeId.HasValue
                ? await _unitOfWork.Resumes.GetResumeWithDetailsAsync(application.ResumeId.Value)
                : null;

            Application = new ApplicationDetailViewModel
            {
                Id = application.Id,
                VacancyId = application.VacancyId,
                VacancyTitle = vacancy?.Title ?? "Не указано",
                CompanyName = vacancy?.Company?.Name ?? "Не указано",

                UserId = application.UserId,
                ApplicantName = user?.FullName ?? application.UserId,
                ApplicantEmail = user?.Email ?? "Не указан",
                ApplicantPhone = user?.PhoneNumber,

                ResumeId = resume?.Id,
                ResumeTitle = resume?.Title,
                Resume = resume,

                Status = application.Status,
                StatusDisplay = application.Status.GetDisplayName(),
                StatusColor = application.Status.GetColorClass(),

                CoverLetter = application.CoverLetter,
                AppliedAt = application.AppliedAt,
                AppliedAtDisplay = FormatDate(application.AppliedAt),
                StatusUpdatedAt = application.StatusUpdatedAt,
                EmployerComment = application.EmployerComment
            };
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

        public async Task<IActionResult> OnPostUpdateStatusAsync(int applicationId, ApplicationStatus newStatus, string? employerComment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);
            if (application == null)
            {
                return NotFound();
            }

            // Проверяем доступ
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(application.VacancyId);
            if (vacancy == null)
            {
                return NotFound();
            }

            var isOwner = vacancy.CreatedById == userId;
            var hasCompanyAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, vacancy.CompanyId);

            if (!isOwner && !hasCompanyAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _unitOfWork.Applications.UpdateStatusAsync(applicationId, newStatus, employerComment);

            TempData["SuccessMessage"] = $"Статус отклика изменен на {newStatus.GetDisplayName()}";
            return RedirectToPage(new { id = applicationId });
        }
    }
}