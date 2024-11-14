using EmailHubApp.Model.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailHubApp.Models
{
    public class OperationsOperation
    {
        public OperationsOperation()
        {
            this.ActionName = string.Empty;
            this.Description = string.Empty;
            this.ActionTime = DateTime.Today;
        }
        public int ID { get; set; }
        public string ActionName { get; set; }
        public string Description { get; set; }
        public DateTime ActionTime { get; set; }
        public int ActionUser { get; set; }
    }
}
