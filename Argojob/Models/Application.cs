using System.ComponentModel.DataAnnotations;
using Agrojob.Data;

namespace Agrojob.Models
{
    /// <summary>
    /// Отклик на вакансию
    /// </summary>
    public class Application
    {
        public int Id { get; set; }

        // Статус отклика
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // Сопроводительное письмо
        [MaxLength(2000)]
        public string? CoverLetter { get; set; }

        // Дата отклика
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        // Дата обновления статуса
        public DateTime? StatusUpdatedAt { get; set; }

        // Комментарий работодателя (например, причина отказа)
        [MaxLength(1000)]
        public string? EmployerComment { get; set; }

        // Внешние ключи
        public int VacancyId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? ResumeId { get; set; } // Какое резюме использовалось

        // Навигационные свойства
        public virtual Vacancy Vacancy { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Resume? Resume { get; set; }
    }

    /// <summary>
    /// Статус отклика
    /// </summary>
    public enum ApplicationStatus
    {
        [Display(Name = "На рассмотрении")]
        Pending = 0,

        [Display(Name = "Просмотрено")]
        Viewed = 1,

        [Display(Name = "Приглашение на собеседование")]
        Invited = 2,

        [Display(Name = "Принят")]
        Accepted = 3,

        [Display(Name = "Отказ")]
        Rejected = 4,

        [Display(Name = "Отозвано соискателем")]
        Withdrawn = 5
    }
}
