using System.ComponentModel.DataAnnotations;
using Agrojob.Models;
using Microsoft.AspNetCore.Identity;

namespace Agrojob.Data
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        // Связь с созданными вакансиями
        public virtual ICollection<Vacancy> CreatedVacancies { get; set; } = new List<Vacancy>();
    }
}
