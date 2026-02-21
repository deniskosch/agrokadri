using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Категория вакансии (Агроном, Ветеринар и т.д.)
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    }
}
