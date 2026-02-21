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
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();

        public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
