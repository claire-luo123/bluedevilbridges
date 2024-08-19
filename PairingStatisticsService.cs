public class PairingStatisticsService
{
    public bool StatisticsAvailable { get; set; } = false;
    public Dictionary<string, int> MenteeYear { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeMajors { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeMinors { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeCertificates { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeState { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeIndustry { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeGraduate { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeInterests { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> MenteeEthnicity { get; set; } = new Dictionary<string, int>();
    public int FirstGen { get; set; }
    public int International { get; set; }

    public DateTime LastUpdated { get; set; }

    public void UpdateStatistics()
    {
        // Update statistics by adding parameters (ie int updatedMentorYear, and then MentorYear = updatedMentorYear)

        LastUpdated = DateTime.Now;
        StatisticsAvailable = true;
    }
}
