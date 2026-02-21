using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Требование к кандидату
    /// </summary>
    public class Requirement
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public int VacancyId { get; set; }
        public virtual Vacancy Vacancy { get; set; }
    }
}
