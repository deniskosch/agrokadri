namespace Agrojob.ViewModels
{
    public class VacancyViewModel
    {
        // Существующие свойства
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string PostedDate { get; set; } = string.Empty;
        public bool IsSeasonal { get; set; }
        public string Description { get; set; } = string.Empty;

        // Дополнительные свойства для панели работодателя
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationsCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedById { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class VacancyDetailViewModel
    {
        // Существующие свойства
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string PostedDate { get; set; } = string.Empty;
        public bool IsSeasonal { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Requirements { get; set; } = new();
        public List<string> Offers { get; set; } = new();
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Дополнительные свойства
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationsCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedById { get; set; }
        public int ViewsCount { get; set; }
        public string? CompanyDescription { get; set; }
        public bool CompanyIsVerified { get; set; }
    }

    // Новые ViewModel для панели работодателя
    public class CompanyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsVerified { get; set; }
        public int VacanciesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RecentApplicationViewModel
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ApplicantId { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string? ApplicantEmail { get; set; }
        public string? ApplicantPhone { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public bool HasResume { get; set; }
        public int? ResumeId { get; set; }
        public string? CoverLetter { get; set; }
    }
}