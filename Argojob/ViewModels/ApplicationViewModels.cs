using Agrojob.Models;

namespace Agrojob.ViewModels
{
    public class ApplicationListItemViewModel
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string? ApplicantPhone { get; set; }
        public string? ResumeTitle { get; set; }
        public int? ResumeId { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string AppliedAtDisplay { get; set; } = string.Empty;
        public string? CoverLetterPreview { get; set; }
    }

    public class ApplicationDetailViewModel
    {
        public int Id { get; set; }

        // Информация о вакансии
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Информация о соискателе
        public string UserId { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string? ApplicantPhone { get; set; }

        // Информация о резюме
        public int? ResumeId { get; set; }
        public string? ResumeTitle { get; set; }
        public Resume? Resume { get; set; }

        // Информация об отклике
        public ApplicationStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string? CoverLetter { get; set; }
        public DateTime AppliedAt { get; set; }
        public string AppliedAtDisplay { get; set; } = string.Empty;
        public DateTime? StatusUpdatedAt { get; set; }
        public string? EmployerComment { get; set; }
    }

    public class ApplicationStatusUpdateModel
    {
        public int ApplicationId { get; set; }
        public ApplicationStatus NewStatus { get; set; }
        public string? EmployerComment { get; set; }
    }

    public static class ApplicationStatusExtensions
    {
        public static string GetDisplayName(this ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Pending => "На рассмотрении",
                ApplicationStatus.Viewed => "Просмотрено",
                ApplicationStatus.Invited => "Приглашение на собеседование",
                ApplicationStatus.Accepted => "Принят",
                ApplicationStatus.Rejected => "Отказ",
                ApplicationStatus.Withdrawn => "Отозвано",
                _ => "Неизвестно"
            };
        }

        public static string GetColorClass(this ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Pending => "warning",
                ApplicationStatus.Viewed => "info",
                ApplicationStatus.Invited => "primary",
                ApplicationStatus.Accepted => "success",
                ApplicationStatus.Rejected => "danger",
                ApplicationStatus.Withdrawn => "secondary",
                _ => "secondary"
            };
        }

        public static string GetIconClass(this ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Pending => "bi-clock-history",
                ApplicationStatus.Viewed => "bi-eye",
                ApplicationStatus.Invited => "bi-envelope",
                ApplicationStatus.Accepted => "bi-check-circle",
                ApplicationStatus.Rejected => "bi-x-circle",
                ApplicationStatus.Withdrawn => "bi-arrow-return-left",
                _ => "bi-question-circle"
            };
        }
    }
}
