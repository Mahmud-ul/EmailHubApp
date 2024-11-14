namespace EmailHubApp.Models
{
    public class UserTypeOperation
    {
        public UserTypeOperation()
        {
            this.Name = String.Empty;
            this.Limit = 0;
            this.IsActive = false;
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public int Limit { get; set; }
        public bool IsActive { get; set; }
    }
}
