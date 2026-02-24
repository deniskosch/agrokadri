using System.Security.Claims;
using Agrojob.Data;
using Agrojob.Models;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace Agrojob.Pages.EmployeeManagement
{
    [Authorize(Roles = "Employee")]
    public class EmployeeManagementMenuModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmployeeManagementMenuModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public List<ResumeViewModel> Resumes { get; set; } = new();
        public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
        public int TotalApplicationsCount { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadResumesAsync(userId);
            await LoadRecentApplicationsAsync(userId);
        }

        private async Task LoadResumesAsync(string userId)
        {
            var resumes = await _unitOfWork.Resumes.FindAsync(r => r.UserId == userId);

            Resumes = resumes.Select(r => new ResumeViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Specialization = r.Category?.Name ?? "Не указана",
                Experience = r.ExperienceYears ?? 0,
                DesiredSalary = r.DesiredSalary ?? "Не указана",
                IsActive = r.IsActive,
                IsPublished = r.IsPublished,
                UpdatedAt = r.UpdatedAt ?? r.CreatedAt
            }).OrderByDescending(r => r.UpdatedAt).ToList();
        }

        private async Task LoadRecentApplicationsAsync(string userId)
        {
            var applications = await _unitOfWork.Applications.GetApplicationsByUserAsync(userId);

            TotalApplicationsCount = applications.Count();

            RecentApplications = applications
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .Select(a => new RecentApplicationViewModel
                {
                    Id = a.Id,
                    VacancyTitle = a.Vacancy.Title,
                    CompanyName = a.Vacancy.Company?.Name ?? "Не указана",
                    AppliedDate = a.AppliedAt,
                    Status = GetStatusDisplay(a.Status),
                    StatusCode = a.Status.ToString().ToLower(),
                    Salary = a.Vacancy.Salary ?? "Не указана",
                    VacancyId = a.VacancyId
                }).ToList();
        }

        private string GetStatusDisplay(ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Pending => "На рассмотрении",
                ApplicationStatus.Viewed => "Просмотрено",
                ApplicationStatus.Invited => "Приглашение",
                ApplicationStatus.Accepted => "Принято",
                ApplicationStatus.Rejected => "Отказ",
                ApplicationStatus.Withdrawn => "Отозвано",
                _ => status.ToString()
            };
        }

        public async Task<IActionResult> OnPostDeleteResumeAsync(int resumeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);

            if (resume != null && resume.UserId == userId)
            {
                await _unitOfWork.Resumes.DeleteAsync(resume);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBecomeEmployerAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            // Проверяем, не является ли пользователь уже работодателем
            if (await _userManager.IsInRoleAsync(user, "Employer"))
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["InfoMessage"] = "Вы уже являетесь работодателем";
                return RedirectToPage("/EmployerManagement/EmployerManagementMenu");
            }

            // Добавляем в роль Employer
            var addResult = await _userManager.AddToRoleAsync(user, "Employer");
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Удаляем из роли Employee (если был)
            if (await _userManager.IsInRoleAsync(user, "Employee"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Employee");
            }

            // Обновляем клеймы пользователя в текущей сессии
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Поздравляем! Теперь вы работодатель. Создайте свою первую компанию.";
            return RedirectToPage("/EmployerManagement/EmployerManagementMenu");
        }
    }

    public class ResumeViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int Experience { get; set; }
        public string DesiredSalary { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RecentApplicationViewModel
    {
        public int Id { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public int VacancyId { get; set; }
    }
}