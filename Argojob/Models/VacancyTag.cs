namespace Agrojob.Models
{
    /// <summary>
    /// Промежуточная таблица для связи Vacancy и Tag (многие-ко-многим)
    /// </summary>
    public class VacancyTag
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public int TagId { get; set; }

        public virtual Vacancy Vacancy { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
