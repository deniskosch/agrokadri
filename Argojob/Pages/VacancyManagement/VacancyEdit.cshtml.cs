using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Agrojob.Models;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Pages.VacancyManagement
{
    [Authorize]
    public class VacancyEditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public VacancyEditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromRoute]
        public int? Id { get; set; }

        public bool IsEditMode => Id.HasValue;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> Companies { get; set; } = new();
        public List<SelectListItem> Locations { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public List<Tag> AllTags { get; set; } = new();

        public class InputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Название вакансии обязательно")]
            [Display(Name = "Название вакансии")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Описание вакансии обязательно")]
            [Display(Name = "Описание")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Зарплата")]
            public string? Salary { get; set; }

            [Display(Name = "Сезонная работа")]
            public bool IsSeasonal { get; set; }

            [Required(ErrorMessage = "Выберите компанию")]
            [Display(Name = "Компания")]
            public int CompanyId { get; set; }

            [Required(ErrorMessage = "Выберите локацию")]
            [Display(Name = "Локация")]
            public int LocationId { get; set; }

            [Required(ErrorMessage = "Выберите категорию")]
            [Display(Name = "Категория")]
            public int CategoryId { get; set; }

            public List<string> Requirements { get; set; } = new();
            public List<string> Offers { get; set; } = new();
            public List<int> SelectedTagIds { get; set; } = new();

            // Контактные данные
            public string? ContactPerson { get; set; }

            [Phone(ErrorMessage = "Неверный формат телефона")]
            public string? ContactPhone { get; set; }

            [EmailAddress(ErrorMessage = "Неверный формат email")]
            public string? ContactEmail { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await LoadDropdownsAsync(userId);

            if (id.HasValue)
            {
                // Режим редактирования - загружаем данные вакансии
                var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(id.Value);

                if (vacancy == null)
                {
                    return NotFound();
                }

                // Проверяем, принадлежит ли вакансия текущему пользователю
                if (vacancy.CreatedById != userId)
                {
                    return Forbid();
                }

                // Заполняем Input модель
                Input.Id = vacancy.Id;
                Input.Title = vacancy.Title;
                Input.Description = vacancy.Description;
                Input.Salary = vacancy.Salary;
                Input.IsSeasonal = vacancy.IsSeasonal;
                Input.CompanyId = vacancy.CompanyId;
                Input.LocationId = vacancy.LocationId;
                Input.CategoryId = vacancy.CategoryId;

                Input.Requirements = vacancy.Requirements?
                    .Select(r => r.Text)
                    .ToList() ?? new();

                Input.Offers = vacancy.Offers?
                    .Select(o => o.Text)
                    .ToList() ?? new();

                Input.SelectedTagIds = vacancy.VacancyTags?
                    .Select(vt => vt.TagId)
                    .ToList() ?? new();

                // Контактные данные из компании
                if (vacancy.Company != null)
                {
                    Input.ContactPerson = vacancy.Company.ContactPerson;
                    Input.ContactPhone = vacancy.Company.ContactPhone;
                    Input.ContactEmail = vacancy.Company.ContactEmail;
                }
            }

            return Page();
        }

        private async Task LoadDropdownsAsync(string userId)
        {
            // Загружаем только компании, к которым пользователь имеет доступ
            var userCompanies = await _unitOfWork.Companies.GetCompaniesByUserAsync(userId);

            Companies = userCompanies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            var locations = await _unitOfWork.Locations.GetAllAsync();
            Locations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Region != null ? $"{l.Name}, {l.Region}" : l.Name
            }).ToList();

            var categories = await _unitOfWork.Categories.GetAllAsync();
            Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            AllTags = (await _unitOfWork.Tags.GetAllAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await LoadDropdownsAsync(userId);
                return Page();
            }

            var userId2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId2))
            {
                return Challenge();
            }

            try
            {
                // Проверяем, имеет ли пользователь доступ к выбранной компании
                var hasAccess = await _unitOfWork.Companies.IsUserInCompanyAsync(userId2, Input.CompanyId);
                if (!hasAccess)
                {
                    ModelState.AddModelError(string.Empty, "У вас нет доступа к этой компании");
                    await LoadDropdownsAsync(userId2);
                    return Page();
                }

                await _unitOfWork.BeginTransactionAsync();

                if (IsEditMode)
                {
                    // РЕДАКТИРОВАНИЕ
                    var vacancy = await _unitOfWork.Vacancies.GetVacancyWithDetailsAsync(Input.Id!.Value);

                    if (vacancy == null || vacancy.CreatedById != userId2)
                    {
                        return NotFound();
                    }

                    // Обновляем основные поля
                    vacancy.Title = Input.Title;
                    vacancy.Description = Input.Description;
                    vacancy.Salary = Input.Salary ?? "Не указана";
                    vacancy.IsSeasonal = Input.IsSeasonal;
                    vacancy.CompanyId = Input.CompanyId;
                    vacancy.LocationId = Input.LocationId;
                    vacancy.CategoryId = Input.CategoryId;

                    await _unitOfWork.Vacancies.UpdateAsync(vacancy);

                    // Обновляем требования
                    await _unitOfWork.Requirements.DeleteAllByVacancyAsync(vacancy.Id);
                    var validRequirements = Input.Requirements?
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => r.Trim())
                        .ToList() ?? new();
                    if (validRequirements.Any())
                    {
                        await _unitOfWork.Requirements.AddRangeToVacancyAsync(vacancy.Id, validRequirements);
                    }

                    // Обновляем условия
                    await _unitOfWork.Offers.DeleteAllByVacancyAsync(vacancy.Id);
                    var validOffers = Input.Offers?
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => o.Trim())
                        .ToList() ?? new();
                    if (validOffers.Any())
                    {
                        await _unitOfWork.Offers.AddRangeToVacancyAsync(vacancy.Id, validOffers);
                    }

                    // Обновляем теги - используем метод из репозитория
                    if (Input.SelectedTagIds != null && Input.SelectedTagIds.Any())
                    {
                        var allTags = await _unitOfWork.Tags.GetAllAsync();
                        var selectedTagNames = allTags
                            .Where(t => Input.SelectedTagIds.Contains(t.Id))
                            .Select(t => t.Name)
                            .ToList();

                        await _unitOfWork.Tags.AddTagsToVacancyAsync(vacancy.Id, selectedTagNames);
                    }

                    TempData["SuccessMessage"] = "Вакансия успешно обновлена";
                }
                else
                {
                    // СОЗДАНИЕ
                    var company = await _unitOfWork.Companies.GetByIdAsync(Input.CompanyId);
                    if (company == null)
                    {
                        ModelState.AddModelError(string.Empty, "Компания не найдена");
                        await LoadDropdownsAsync(userId2);
                        return Page();
                    }

                    // Обновляем контактные данные компании
                    bool companyUpdated = false;
                    if (!string.IsNullOrWhiteSpace(Input.ContactPerson) && Input.ContactPerson != company.ContactPerson)
                    {
                        company.ContactPerson = Input.ContactPerson;
                        companyUpdated = true;
                    }
                    if (!string.IsNullOrWhiteSpace(Input.ContactPhone) && Input.ContactPhone != company.ContactPhone)
                    {
                        company.ContactPhone = Input.ContactPhone;
                        companyUpdated = true;
                    }
                    if (!string.IsNullOrWhiteSpace(Input.ContactEmail) && Input.ContactEmail != company.ContactEmail)
                    {
                        company.ContactEmail = Input.ContactEmail;
                        companyUpdated = true;
                    }

                    if (companyUpdated)
                    {
                        await _unitOfWork.Companies.UpdateAsync(company);
                    }

                    // Создаем вакансию
                    var vacancy = new Vacancy
                    {
                        Title = Input.Title,
                        Description = Input.Description,
                        Salary = Input.Salary ?? "Не указана",
                        PostedDate = DateTime.UtcNow,
                        IsSeasonal = Input.IsSeasonal,
                        IsActive = true,
                        ViewsCount = 0,
                        CompanyId = Input.CompanyId,
                        LocationId = Input.LocationId,
                        CategoryId = Input.CategoryId,
                        CreatedById = userId2
                    };

                    var vacancyId = await _unitOfWork.Vacancies.AddAsync(vacancy);

                    // Добавляем требования
                    var validRequirements = Input.Requirements?
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => r.Trim())
                        .ToList() ?? new();
                    if (validRequirements.Any())
                    {
                        await _unitOfWork.Requirements.AddRangeToVacancyAsync(vacancyId, validRequirements);
                    }

                    // Добавляем условия
                    var validOffers = Input.Offers?
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => o.Trim())
                        .ToList() ?? new();
                    if (validOffers.Any())
                    {
                        await _unitOfWork.Offers.AddRangeToVacancyAsync(vacancyId, validOffers);
                    }

                    // Добавляем теги
                    if (Input.SelectedTagIds != null && Input.SelectedTagIds.Any())
                    {
                        var allTags = await _unitOfWork.Tags.GetAllAsync();
                        var selectedTagNames = allTags
                            .Where(t => Input.SelectedTagIds.Contains(t.Id))
                            .Select(t => t.Name)
                            .ToList();

                        await _unitOfWork.Tags.AddTagsToVacancyAsync(vacancyId, selectedTagNames);
                    }

                    Input.Id = vacancyId;
                    TempData["SuccessMessage"] = "Вакансия успешно создана";
                }

                await _unitOfWork.CommitTransactionAsync();
                return RedirectToPage("/VacancyDetail", new { id = Input.Id });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.Message}");

                var userId3 = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await LoadDropdownsAsync(userId3);
                return Page();
            }
        }
    }
}