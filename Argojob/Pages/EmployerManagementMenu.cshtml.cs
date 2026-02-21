using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agrojob.UoW;

namespace Agrojob.Pages
{
    [Authorize]
    public class EmployerManagementMenuModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployerManagementMenuModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public int VacanciesCount { get; set; }
        public int ApplicationsCount { get; set; }
        public int CompaniesCount { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadCountersAsync(userId);
        }

        private async Task LoadCountersAsync(string userId)
        {
            // Получаем все компании пользователя
            var companies = await _unitOfWork.Companies.GetCompaniesByUserAsync(userId);
            CompaniesCount = companies.Count();

            // Получаем все вакансии пользователя
            var vacancies = await _unitOfWork.Vacancies.FindAsync(v => v.CreatedById == userId);
            VacanciesCount = vacancies.Count();

            // Получаем все отклики на вакансии пользователя
            var companyIds = companies.Select(c => c.Id).ToList();
            var userVacancies = vacancies.ToList();

            int applicationsTotal = 0;
            foreach (var vacancy in userVacancies)
            {
                var apps = await _unitOfWork.Applications.GetApplicationsByVacancyAsync(vacancy.Id);
                applicationsTotal += apps.Count();
            }

            ApplicationsCount = applicationsTotal;
        }
    }
}