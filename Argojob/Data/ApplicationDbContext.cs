using Agrojob.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<VacancyTag> VacancyTags { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка связей и индексов

            // Vacancy - Company (один ко многим)
            builder.Entity<Vacancy>()
                .HasOne(v => v.Company)
                .WithMany(c => c.Vacancies)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vacancy - Location
            builder.Entity<Vacancy>()
                .HasOne(v => v.Location)
                .WithMany(l => l.Vacancies)
                .HasForeignKey(v => v.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vacancy - Category
            builder.Entity<Vacancy>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Vacancies)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vacancy - CreatedBy (User)
            builder.Entity<Vacancy>()
                .HasOne(v => v.CreatedBy)
                .WithMany(u => u.CreatedVacancies)
                .HasForeignKey(v => v.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // VacancyTag - составной ключ для избежания дубликатов
            builder.Entity<VacancyTag>()
                .HasIndex(vt => new { vt.VacancyId, vt.TagId })
                .IsUnique();

            // Requirement - связь с Vacancy
            builder.Entity<Requirement>()
                .HasOne(r => r.Vacancy)
                .WithMany(v => v.Requirements)
                .HasForeignKey(r => r.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Offer - связь с Vacancy
            builder.Entity<Offer>()
                .HasOne(o => o.Vacancy)
                .WithMany(v => v.Offers)
                .HasForeignKey(o => o.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для поиска
            builder.Entity<Vacancy>()
                .HasIndex(v => v.IsActive);

            builder.Entity<Vacancy>()
                .HasIndex(v => v.IsSeasonal);

            builder.Entity<Vacancy>()
                .HasIndex(v => v.PostedDate);

            builder.Entity<Company>()
                .HasIndex(c => c.Name);

            builder.Entity<Location>()
                .HasIndex(l => l.Name);

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }

    }
}
