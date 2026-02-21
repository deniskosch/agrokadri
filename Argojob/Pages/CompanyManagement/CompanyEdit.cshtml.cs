using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Agrojob.Models;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Agrojob.Pages.CompanyManagement
{
    [Authorize]
    public class CompanyEditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyEditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromRoute]
        public int? Id { get; set; }

        public bool IsEditMode => Id.HasValue;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Название компании обязательно")]
            [Display(Name = "Название компании")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Описание")]
            public string? Description { get; set; }

            [Display(Name = "Контактное лицо")]
            public string? ContactPerson { get; set; }

            [Display(Name = "Телефон")]
            [Phone(ErrorMessage = "Неверный формат телефона")]
            public string? ContactPhone { get; set; }

            [Display(Name = "Email")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            public string? ContactEmail { get; set; }

            [Display(Name = "Проверенная компания")]
            public bool IsVerified { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (id.HasValue)
            {
                // Режим редактирования - загружаем данные компании
                var company = await _unitOfWork.Companies.GetByIdAsync(id.Value);

                if (company == null)
                {
                    return NotFound();
                }

                // Проверяем, имеет ли пользователь доступ к компании
                var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, id.Value);
                if (!hasAccess && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Заполняем Input модель
                Input.Id = company.Id;
                Input.Name = company.Name;
                Input.Description = company.Description;
                Input.ContactPerson = company.ContactPerson;
                Input.ContactPhone = company.ContactPhone;
                Input.ContactEmail = company.ContactEmail;
                Input.IsVerified = company.IsVerified;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (IsEditMode)
                {
                    // РЕДАКТИРОВАНИЕ
                    var company = await _unitOfWork.Companies.GetByIdAsync(Input.Id!.Value);

                    if (company == null)
                    {
                        return NotFound();
                    }

                    // Проверяем доступ
                    var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId, Input.Id.Value);
                    if (!hasAccess && !User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    // Обновляем поля
                    company.Name = Input.Name;
                    company.Description = Input.Description;
                    company.ContactPerson = Input.ContactPerson;
                    company.ContactPhone = Input.ContactPhone;
                    company.ContactEmail = Input.ContactEmail;

                    // Только админ может менять статус верификации
                    if (User.IsInRole("Admin"))
                    {
                        company.IsVerified = Input.IsVerified;
                    }

                    await _unitOfWork.Companies.UpdateAsync(company);
                    TempData["SuccessMessage"] = "Компания успешно обновлена";
                }
                else
                {
                    // СОЗДАНИЕ
                    // Проверяем, не существует ли уже компания с таким названием
                    var existingCompany = await _unitOfWork.Companies.GetCompanyByNameAsync(Input.Name);
                    if (existingCompany != null)
                    {
                        ModelState.AddModelError("Input.Name", "Компания с таким названием уже существует");
                        return Page();
                    }

                    // Создаем компанию
                    var company = new Company
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        ContactPerson = Input.ContactPerson,
                        ContactPhone = Input.ContactPhone,
                        ContactEmail = Input.ContactEmail,
                        IsVerified = User.IsInRole("Admin") ? Input.IsVerified : false
                    };

                    var companyId = await _unitOfWork.Companies.AddAsync(company);

                    // Добавляем текущего пользователя как владельца компании
                    await _unitOfWork.Companies.AddUserToCompanyAsync(userId, companyId, "Admin");

                    Input.Id = companyId;
                    TempData["SuccessMessage"] = "Компания успешно создана";
                }

                await _unitOfWork.CommitTransactionAsync();
                return RedirectToPage("MyCompanies");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.Message}");
                return Page();
            }
        }
    }
}