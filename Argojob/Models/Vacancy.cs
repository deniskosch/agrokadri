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

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Зарплата (храним как строку, как в вашей модели)
        [MaxLength(100)]
        public string Salary { get; set; } = string.Empty;

        public int ViewsCount { get; set; }

        // Дата публикации
        public DateTime PostedDate { get; set; }

        // Флаги
        public bool IsSeasonal { get; set; }
        public bool IsActive { get; set; } = true;

        // Внешние ключи
        public int CompanyId { get; set; }
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
        public string? CreatedById { get; set; } // Кто создал (IdentityUser)

        // Навигационные свойства
        public virtual Company Company { get; set; }
        public virtual Location Location { get; set; }
        public virtual Category Category { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        // Коллекции для связанных данных
        public virtual ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
