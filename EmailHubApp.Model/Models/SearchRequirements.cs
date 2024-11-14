using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Model.Models
{
    public class SearchRequirements
    {
        public SearchRequirements() 
        {
            this.CX = string.Empty;
            this.ApiKey = string.Empty;
            this.SearchCount = 0;
            this.IsActive = true;
            this.LastUpdatedDay = DateTime.Today;
        }
        public int ID { get; set; }
        public string CX { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        public int SearchCount { get; set; }
        public DateTime LastUpdatedDay { get; set; }
    }
}