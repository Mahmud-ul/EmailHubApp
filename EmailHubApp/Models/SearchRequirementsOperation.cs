namespace EmailHubApp.Models
{
    public class SearchRequirementsOperation
    {
        public int ID { get; set; }
        public string CX { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        public int SearchCount { get; set; }
        public DateTime LastUpdatedDay { get; set; }
    }
}
