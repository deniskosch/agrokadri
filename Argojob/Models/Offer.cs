using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Предложение/условие работы
    /// </summary>
    public class Offer
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public int VacancyId { get; set; }
        public virtual Vacancy Vacancy { get; set; }
    }
}
