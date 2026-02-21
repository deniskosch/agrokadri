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
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== НАСТРОЙКИ ДЛЯ VACANCY =====

            // Vacancy - Company
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

            // Индексы для поиска Vacancy
            builder.Entity<Vacancy>()
                .HasIndex(v => v.IsActive);

            builder.Entity<Vacancy>()
                .HasIndex(v => v.IsSeasonal);

            builder.Entity<Vacancy>()
                .HasIndex(v => v.PostedDate);

            // ===== НАСТРОЙКИ ДЛЯ COMPANY =====

            builder.Entity<Company>()
                .HasIndex(c => c.Name);

            // ===== НАСТРОЙКИ ДЛЯ LOCATION =====

            builder.Entity<Location>()
                .HasIndex(l => l.Name);

            // ===== НАСТРОЙКИ ДЛЯ CATEGORY =====

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // ===== НАСТРОЙКИ ДЛЯ TAG =====

            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // ===== НАСТРОЙКИ ДЛЯ COMPANYUSER =====

            builder.Entity<CompanyUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.CompanyUsers)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CompanyUser>()
                .HasOne(cu => cu.Company)
                .WithMany(c => c.CompanyUsers)
                .HasForeignKey(cu => cu.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CompanyUser>()
                .HasIndex(cu => new { cu.UserId, cu.CompanyId })
                .IsUnique();

            builder.Entity<CompanyUser>()
                .HasIndex(cu => cu.UserId);

            builder.Entity<CompanyUser>()
                .HasIndex(cu => cu.CompanyId);

            // ===== НАСТРОЙКИ ДЛЯ RESUME =====

            builder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict вместо Cascade

            builder.Entity<Resume>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Resumes)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Resume>()
                .HasIndex(r => r.UserId);

            builder.Entity<Resume>()
                .HasIndex(r => r.IsActive);

            builder.Entity<Resume>()
                .HasIndex(r => r.CategoryId);

            // ===== НАСТРОЙКИ ДЛЯ APPLICATION =====

            // Application - Vacancy (Restrict вместо Cascade)
            builder.Entity<Application>()
                .HasOne(a => a.Vacancy)
                .WithMany(v => v.Applications)
                .HasForeignKey(a => a.VacancyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application - User (Restrict вместо Cascade)
            builder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application - Resume (SetNull - безопасно)
            builder.Entity<Application>()
                .HasOne(a => a.Resume)
                .WithMany(r => r.Applications)
                .HasForeignKey(a => a.ResumeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Application>()
                .HasIndex(a => a.UserId);

            builder.Entity<Application>()
                .HasIndex(a => a.VacancyId);

            builder.Entity<Application>()
                .HasIndex(a => a.Status);

            // Составной уникальный индекс для предотвращения дублирования откликов
            builder.Entity<Application>()
                .HasIndex(a => new { a.VacancyId, a.UserId })
                .IsUnique();
        }
    }
}