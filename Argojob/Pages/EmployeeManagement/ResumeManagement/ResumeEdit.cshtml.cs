using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Agrojob.Models;
using Agrojob.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Agrojob.Pages.EmployeeManagement.ResumeManagement
{
    [Authorize]
    public class ResumeEditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ResumeEditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [FromRoute]
        public int? Id { get; set; }

        public bool IsEditMode => Id.HasValue;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> Categories { get; set; } = new();

        public class InputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Название резюме обязательно")]
            [Display(Name = "Название резюме")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Укажите ФИО")]
            [Display(Name = "ФИО")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Дата рождения")]
            [DataType(DataType.Date)]
            public DateTime? BirthDate { get; set; }

            [Display(Name = "Телефон")]
            [Phone(ErrorMessage = "Неверный формат телефона")]
            public string? Phone { get; set; }

            [Display(Name = "Email")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            public string? Email { get; set; }

            [Display(Name = "Город")]
            public string? Location { get; set; }

            [Display(Name = "Опыт работы (лет)")]
            [Range(0, 60, ErrorMessage = "Введите корректное значение")]
            public int? ExperienceYears { get; set; }

            [Display(Name = "Образование")]
            public string? Education { get; set; }

            [Display(Name = "Опыт работы")]
            public string? Experience { get; set; }

            [Display(Name = "Навыки")]
            public string? Skills { get; set; }

            [Display(Name = "О себе")]
            public string? About { get; set; }

            [Display(Name = "Желаемая зарплата")]
            public string? DesiredSalary { get; set; }

            [Display(Name = "Готов к переезду")]
            public bool ReadyToRelocate { get; set; }

            [Display(Name = "Готов к командировкам")]
            public bool ReadyForBusinessTrips { get; set; }

            [Display(Name = "Активно")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Опубликовано")]
            public bool IsPublished { get; set; } = true;

            [Display(Name = "Категория")]
            public int? CategoryId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await LoadDropdownsAsync();

            if (id.HasValue)
            {
                // Режим редактирования - загружаем данные резюме
                var resume = await _unitOfWork.Resumes.GetResumeWithDetailsAsync(id.Value);

                if (resume == null)
                {
                    return NotFound();
                }

                // Проверяем, принадлежит ли резюме текущему пользователю
                if (resume.UserId != userId)
                {
                    return Forbid();
                }

                // Заполняем Input модель
                Input.Id = resume.Id;
                Input.Title = resume.Title;
                Input.FullName = resume.FullName;
                Input.BirthDate = resume.BirthDate;
                Input.Phone = resume.Phone;
                Input.Email = resume.Email;
                Input.Location = resume.Location;
                Input.ExperienceYears = resume.ExperienceYears;
                Input.Education = resume.Education;
                Input.Experience = resume.Experience;
                Input.Skills = resume.Skills;
                Input.About = resume.About;
                Input.DesiredSalary = resume.DesiredSalary;
                Input.ReadyToRelocate = resume.ReadyToRelocate;
                Input.ReadyForBusinessTrips = resume.ReadyForBusinessTrips;
                Input.IsActive = resume.IsActive;
                Input.IsPublished = resume.IsPublished;
                Input.CategoryId = resume.CategoryId;
            }

            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            // Добавляем пустой элемент в начало
            Categories.Insert(0, new SelectListItem { Value = "", Text = "-- Выберите категорию --" });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
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
                    var resume = await _unitOfWork.Resumes.GetByIdAsync(Input.Id!.Value);

                    if (resume == null || resume.UserId != userId)
                    {
                        return NotFound();
                    }

                    // Обновляем поля
                    resume.Title = Input.Title;
                    resume.FullName = Input.FullName;
                    resume.BirthDate = Input.BirthDate;
                    resume.Phone = Input.Phone;
                    resume.Email = Input.Email;
                    resume.Location = Input.Location;
                    resume.ExperienceYears = Input.ExperienceYears;
                    resume.Education = Input.Education;
                    resume.Experience = Input.Experience;
                    resume.Skills = Input.Skills;
                    resume.About = Input.About;
                    resume.DesiredSalary = Input.DesiredSalary;
                    resume.ReadyToRelocate = Input.ReadyToRelocate;
                    resume.ReadyForBusinessTrips = Input.ReadyForBusinessTrips;
                    resume.IsActive = Input.IsActive;
                    resume.IsPublished = Input.IsPublished;
                    resume.CategoryId = Input.CategoryId;
                    resume.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.Resumes.UpdateAsync(resume);
                    TempData["SuccessMessage"] = "Резюме успешно обновлено";
                }
                else
                {
                    // СОЗДАНИЕ
                    var resume = new Resume
                    {
                        Title = Input.Title,
                        FullName = Input.FullName,
                        BirthDate = Input.BirthDate,
                        Phone = Input.Phone,
                        Email = Input.Email,
                        Location = Input.Location,
                        ExperienceYears = Input.ExperienceYears,
                        Education = Input.Education,
                        Experience = Input.Experience,
                        Skills = Input.Skills,
                        About = Input.About,
                        DesiredSalary = Input.DesiredSalary,
                        ReadyToRelocate = Input.ReadyToRelocate,
                        ReadyForBusinessTrips = Input.ReadyForBusinessTrips,
                        IsActive = Input.IsActive,
                        IsPublished = Input.IsPublished,
                        CategoryId = Input.CategoryId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    var resumeId = await _unitOfWork.Resumes.AddAsync(resume);
                    Input.Id = resumeId;
                    TempData["SuccessMessage"] = "Резюме успешно создано";
                }

                await _unitOfWork.CommitTransactionAsync();
                return RedirectToPage("/EmployeeManagement/EmployeeManagementMenu");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }
    }
}