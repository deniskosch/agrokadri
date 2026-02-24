using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Agrojob.UoW;
using Agrojob.Data;

namespace Agrojob.Pages.EmployerManagement.CompanyManagement
{
    [Authorize]
    public class CompanyUsersModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyUsersModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [FromRoute]
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public int CompanyId => Id;
        public List<CompanyUserViewModel> Users { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Проверяем доступ к компании
            var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, id);
            if (!hasAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var company = await _unitOfWork.Companies.GetCompanyWithVacanciesAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            CompanyName = company.Name;

            // Загружаем пользователей компании через новый метод
            var companyUsers = await _unitOfWork.Companies.GetCompanyUsersWithDetailsAsync(id);

            Users = companyUsers.Select(cu => new CompanyUserViewModel
            {
                UserId = cu.UserId,
                Email = cu.User?.Email ?? "Неизвестно",
                FullName = cu.User?.FullName,
                Role = cu.Role ?? "Viewer",
                JoinedAt = cu.JoinedAt
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAddUserAsync(int companyId, string email, string role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            // Проверяем доступ
            var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(currentUserId, companyId);
            if (!hasAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Ищем пользователя по email через UserManager
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь с таким email не найден";
                return RedirectToPage(new { id = companyId });
            }

            // Проверяем, не привязан ли уже пользователь
            var isInCompany = await _unitOfWork.Companies.IsUserInCompanyAsync(user.Id, companyId);
            if (isInCompany)
            {
                TempData["ErrorMessage"] = "Пользователь уже состоит в компании";
                return RedirectToPage(new { id = companyId });
            }

            // Добавляем пользователя
            await _unitOfWork.Companies.AddUserToCompanyAsync(user.Id, companyId, role);
            TempData["SuccessMessage"] = "Пользователь успешно добавлен";

            return RedirectToPage(new { id = companyId });
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(int companyId, string userId, string newRole)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            // Проверяем доступ
            var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(currentUserId, companyId);
            if (!hasAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Нельзя менять роль владельца (Admin) - нужно дополнительно проверять
            var currentRole = await _unitOfWork.Companies.GetUserRoleInCompanyAsync(userId, companyId);
            if (currentRole == "Admin" && currentUserId != userId)
            {
                TempData["ErrorMessage"] = "Нельзя изменить роль владельца компании";
                return RedirectToPage(new { id = companyId });
            }

            await _unitOfWork.Companies.UpdateUserRoleAsync(userId, companyId, newRole);
            TempData["SuccessMessage"] = "Роль пользователя обновлена";

            return RedirectToPage(new { id = companyId });
        }

        public async Task<IActionResult> OnPostRemoveUserAsync(int companyId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            // Проверяем доступ
            var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(currentUserId, companyId);
            if (!hasAccess && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Нельзя удалить самого себя
            if (currentUserId == userId)
            {
                TempData["ErrorMessage"] = "Вы не можете удалить себя из компании";
                return RedirectToPage(new { id = companyId });
            }

            // Нельзя удалить владельца (Admin)
            var userRole = await _unitOfWork.Companies.GetUserRoleInCompanyAsync(userId, companyId);
            if (userRole == "Admin")
            {
                TempData["ErrorMessage"] = "Нельзя удалить владельца компании";
                return RedirectToPage(new { id = companyId });
            }

            await _unitOfWork.Companies.RemoveUserFromCompanyAsync(userId, companyId);
            TempData["SuccessMessage"] = "Пользователь удален из компании";

            return RedirectToPage(new { id = companyId });
        }
    }

    public class CompanyUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}