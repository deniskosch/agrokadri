namespace Agrojob.ViewModels
{

    public class VacancyViewModel
    {
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
    }

    public class VacancyDetailViewModel
    {
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
    }
}
