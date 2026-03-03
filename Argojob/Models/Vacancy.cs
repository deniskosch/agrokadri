using System.ComponentModel.DataAnnotations;
using Agrojob.Data;

namespace Agrojob.Models
{
    /// <summary>
    /// Вакансия
    /// </summary>
    public class Vacancy
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Тип зарплаты: 0 - Договорная, 1 - Фиксированная, 2 - Диапазон
        public SalaryType SalaryType { get; set; }

        // Фиксированная сумма (используется при SalaryType = Фиксированная)
        public long? FixedSalary { get; set; }

        // Диапазон зарплаты от (используется при SalaryType = Диапазон)
        public long? SalaryFrom { get; set; }

        // Диапазон зарплаты до (используется при SalaryType = Диапазон)
        public long? SalaryTo { get; set; }

        public int ViewsCount { get; set; }

        // Дата публикации
        public DateTime PostedDate { get; set; }

        // Флаги
        public bool IsSeasonal { get; set; }
        public bool IsActive { get; set; } = true;

        // Строковые поля вместо таблиц
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        // Внешние ключи
        public int CompanyId { get; set; }
        public string? CreatedById { get; set; } // Кто создал (IdentityUser)

        // Навигационные свойства
        public virtual Company Company { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        // Коллекции для связанных данных
        public virtual ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }

    /// <summary>
    /// Тип зарплаты
    /// </summary>
    public enum SalaryType
    {
        [Display(Name = "Договорная")]
        Negotiable = 0,

        [Display(Name = "Фиксированная")]
        Fixed = 1,

        [Display(Name = "Диапазон")]
        Range = 2
    }
}