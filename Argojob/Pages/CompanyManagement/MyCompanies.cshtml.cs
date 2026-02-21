using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agrojob.Models;
using Agrojob.UoW;

namespace Agrojob.Pages.CompanyManagement
{
    [Authorize]
    public class MyCompaniesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public MyCompaniesModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromQuery]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 9;
        public int TotalPages { get; set; }

        public int TotalCount { get; set; }
        public int VerifiedCount { get; set; }
        public int TotalVacancies { get; set; }

        public List<MyCompanyViewModel> Companies { get; set; } = new();
        public int CurrentPage => Page;

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await LoadCompaniesAsync(userId);
        }

        private async Task LoadCompaniesAsync(string userId)
        {
            // Получаем все компании пользователя
            var userCompanies = await _unitOfWork.Companies.GetCompaniesByUserAsync(userId);
            var companiesList = userCompanies.ToList();

            // Считаем статистику
            TotalCount = companiesList.Count;
            VerifiedCount = companiesList.Count(c => c.IsVerified);

            // Считаем общее количество вакансий по всем компаниям
            foreach (var company in companiesList)
            {
                TotalVacancies += await _unitOfWork.Companies.GetVacanciesCountAsync(company.Id);
            }

            // Пагинация
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (Page < 1) Page = 1;
            if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

            var pageCompanies = companiesList
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Маппим в ViewModel
            var companyViewModels = new List<MyCompanyViewModel>();
            foreach (var company in pageCompanies)
            {
                companyViewModels.Add(new MyCompanyViewModel
                {
                    Id = company.Id,
                    Name = company.Name,
                    Description = company.Description,
                    ContactPerson = company.ContactPerson,
                    ContactPhone = company.ContactPhone,
                    ContactEmail = company.ContactEmail,
                    IsVerified = company.IsVerified,
                    VacanciesCount = await _unitOfWork.Companies.GetVacanciesCountAsync(company.Id)
                });
            }

            Companies = companyViewModels;
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Проверяем, имеет ли пользователь доступ к компании
            var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, id);
            if (!hasAccess)
            {
                return Forbid();
            }

            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            await _unitOfWork.Companies.DeleteAsync(id);
            TempData["SuccessMessage"] = "Компания успешно удалена";

            return RedirectToPage();
        }
    }

    public class MyCompanyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsVerified { get; set; }
        public int VacanciesCount { get; set; }
    }
}