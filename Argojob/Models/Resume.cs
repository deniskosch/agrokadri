using System.ComponentModel.DataAnnotations;
using Agrojob.Data;

namespace Agrojob.Models
{
    /// <summary>
    /// Резюме соискателя
    /// </summary>
    public class Resume
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty; // Например: "Опытный агроном"

        [Required]
        public string FullName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; } // Город проживания

        // Опыт работы
        public int? ExperienceYears { get; set; } // Общий стаж в годах

        [MaxLength(1000)]
        public string? Education { get; set; } // Образование

        [MaxLength(1000)]
        public string? Experience { get; set; } // Опыт работы (текст)

        [MaxLength(1000)]
        public string? Skills { get; set; } // Навыки

        [MaxLength(1000)]
        public string? About { get; set; } // О себе

        // Желаемая зарплата
        [MaxLength(100)]
        public string? DesiredSalary { get; set; }

        // Готовность к переезду/командировкам
        public bool ReadyToRelocate { get; set; }
        public bool ReadyForBusinessTrips { get; set; }

        // Статус резюме
        public bool IsActive { get; set; } = true;
        public bool IsPublished { get; set; } = true; // Опубликовано или черновик

        // Дата создания и обновления
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Внешние ключи
        public string UserId { get; set; } = string.Empty;
        public int? CategoryId { get; set; } // Желаемая категория

        // Навигационные свойства
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Category? Category { get; set; }

        // Связь с откликами
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
