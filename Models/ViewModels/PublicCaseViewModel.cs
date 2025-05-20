public class PublicCaseViewModel
{
    public string CaseNumber { get; set; }
    public string Title { get; set; }
    public string PlaintiffName { get; set; }
    public string DefendantName { get; set; }
    public string Status { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? UpcomingHearing { get; set; }
    public DateTime? AppointmentDate { get; set; }
}