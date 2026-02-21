using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Тег (для поиска и фильтрации)
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Связь многие-ко-многим через промежуточную таблицу
        public virtual ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
    }
}
