using System.ComponentModel.DataAnnotations;
using Agrojob.Data;

namespace Agrojob.Models
{
    /// <summary>
    /// Связь между компаниями и пользователями (многие-ко-многим)
    /// </summary>
    public class CompanyUser
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        // Роль пользователя в компании (Admin, Manager, Viewer и т.д.)
        [MaxLength(50)]
        public string? Role { get; set; }

        // Дата добавления пользователя в компанию
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
    }
}