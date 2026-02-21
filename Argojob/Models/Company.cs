using System.ComponentModel.DataAnnotations;

namespace Agrojob.Models
{
    /// <summary>
    /// Компания-работодатель
    /// </summary>
    public class Company
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        public bool IsVerified { get; set; }

        // Связь с вакансиями
        public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();

        // Связь с пользователями (многие-ко-многим)
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
    }
}