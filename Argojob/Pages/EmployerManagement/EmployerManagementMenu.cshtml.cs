using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agrojob.UoW;
using Agrojob.Models;
using Microsoft.AspNetCore.Identity;
using Agrojob.Data;
using Agrojob.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Agrojob.Pages.EmployerManagement
{
    [Authorize(Roles = "Employer")]
    public class EmployerManagementMenuModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmployerManagementMenuModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedCompanyId { get; set; }

        public List<CompanyViewModel> Companies { get; set; } = new();
        public List<VacancyViewModel> Vacancies { get; set; } = new();
        public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
        public SelectList CompanySelectList { get; set; }
        public CompanyViewModel SelectedCompany { get; set; }

        // Счетчики для карточек
        public int CompaniesCount => Companies.Count;
        public int VacanciesCount => Vacancies.Count;
        public int ApplicationsCount { get; set; }
        public bool HasCompanies => Companies.Any();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadCompaniesAsync(userId);

            // Если компания не выбрана, но есть компании - выбираем первую
            if (!SelectedCompanyId.HasValue && Companies.Any())
            {
                SelectedCompanyId = Companies.First().Id;
            }

            if (SelectedCompanyId.HasValue)
            {
                SelectedCompany = Companies.FirstOrDefault(c => c.Id == SelectedCompanyId.Value);
                await LoadVacanciesAsync(SelectedCompanyId.Value);
                await LoadRecentApplicationsAsync(SelectedCompanyId.Value);
            }

            // Общее количество откликов (по всем компаниям)
            ApplicationsCount = RecentApplications.Count;
        }

        public async Task<IActionResult> OnPostSelectCompanyAsync(int companyId)
        {
            return RedirectToPage(new { SelectedCompanyId = companyId });
        }

        private async Task LoadCompaniesAsync(string userId)
        {
            var companies = await _unitOfWork.Companies.GetCompaniesByUserAsync(userId);

            foreach (var company in companies)
            {
                var vacanciesCount = await _unitOfWork.Companies.GetVacanciesCountAsync(company.Id);

                Companies.Add(new CompanyViewModel
                {
                    Id = company.Id,
                    Name = company.Name,
                    ContactPerson = company.ContactPerson,
                    ContactPhone = company.ContactPhone,
                    ContactEmail = company.ContactEmail,
                    Description = company.Description,
                    IsVerified = company.IsVerified,
                    VacanciesCount = vacanciesCount
                });
            }
        }

        private async Task LoadVacanciesAsync(int companyId)
        {
            var vacancies = await _unitOfWork.Vacancies.FindAsync(v => v.CompanyId == companyId);

            foreach (var vacancy in vacancies.OrderByDescending(v => v.PostedDate))
            {
                var company = await _unitOfWork.Companies.GetByIdAsync(vacancy.CompanyId);
                var location = await _unitOfWork.Locations.GetByIdAsync(vacancy.LocationId);
                var category = await _unitOfWork.Categories.GetByIdAsync(vacancy.CategoryId);
                var applications = await _unitOfWork.Applications.GetApplicationsByVacancyAsync(vacancy.Id);

                Vacancies.Add(new VacancyViewModel
                {
                    Id = vacancy.Id,
                    Title = vacancy.Title,
                    Company = company?.Name ?? "Не указано",
                    Location = location?.Name ?? "Не указано",
                    Salary = vacancy.Salary,
                    Category = category?.Name ?? "Не указано",
                    PostedDate = vacancy.PostedDate.ToString("dd.MM.yyyy"),
                    IsSeasonal = vacancy.IsSeasonal,
                    IsActive = vacancy.IsActive,
                    ApplicationsCount = applications.Count(),
                    CompanyId = vacancy.CompanyId
                });
            }
        }

        private async Task LoadRecentApplicationsAsync(int companyId)
        {
            var vacancies = await _unitOfWork.Vacancies.FindAsync(v => v.CompanyId == companyId);
            var vacancyIds = vacancies.Select(v => v.Id).ToList();

            if (!vacancyIds.Any())
            {
                return;
            }

            var allApplications = new List<Application>();
            foreach (var vacancyId in vacancyIds)
            {
                var apps = await _unitOfWork.Applications.GetApplicationsByVacancyAsync(vacancyId);
                allApplications.AddRange(apps);
            }

            var recentApps = allApplications
                .OrderByDescending(a => a.AppliedAt)
                .Take(10)
                .ToList();

            foreach (var app in recentApps)
            {
                var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(app.VacancyId);
                var company = vacancy != null ? await _unitOfWork.Companies.GetByIdAsync(vacancy.CompanyId) : null;
                var user = await _userManager.FindByIdAsync(app.UserId);

                RecentApplications.Add(new RecentApplicationViewModel
                {
                    Id = app.Id,
                    VacancyId = app.VacancyId,
                    VacancyTitle = vacancy?.Title ?? "Не указано",
                    CompanyName = company?.Name ?? "Не указано",
                    ApplicantName = user?.FullName ?? app.UserId,
                    ApplicantEmail = user?.Email,
                    AppliedAt = app.AppliedAt,
                    Status = GetStatusDisplay(app.Status),
                    StatusCode = app.Status.ToString().ToLower(),
                    HasResume = app.ResumeId.HasValue
                });
            }
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

        public async Task<IActionResult> OnPostBecomeEmployeeAsync()
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
            if (await _userManager.IsInRoleAsync(user, "Employee"))
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["InfoMessage"] = "Вы уже являетесь соискателем";
                return RedirectToPage("/EmployeeManagement/EmployeeManagementMenu");
            }

            // Добавляем в роль Employee
            var addResult = await _userManager.AddToRoleAsync(user, "Employee");
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Удаляем из роли Employer (если был)
            if (await _userManager.IsInRoleAsync(user, "Employer"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Employer");
            }

            // Обновляем клеймы пользователя в текущей сессии
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Поздравляем! Теперь вы соискатель!";
            return RedirectToPage("/EmployeeManagement/EmployeeManagementMenu");
        }

        public async Task<IActionResult> OnPostActivateVacancyAsync(int vacancyId)
        {
            var res = await _unitOfWork.Vacancies.PublishVacancyAsync(vacancyId);
            await _unitOfWork.CompleteAsync();

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostDeactivateVacancyAsync(int vacancyId)
        {
            var res = await _unitOfWork.Vacancies.ArchiveVacancyAsync(vacancyId);
            await _unitOfWork.CompleteAsync();

            return RedirectToPage();
        }
    }

    public class CompanyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public int VacanciesCount { get; set; }
    }

    public class RecentApplicationViewModel
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string? ApplicantEmail { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public bool HasResume { get; set; }
    }
}