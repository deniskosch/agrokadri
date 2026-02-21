using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Локация (город/регион)
    /// </summary>
    public class Location
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Region { get; set; } // Область/край

        public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    }
}
