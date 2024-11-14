using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Model.Models
{
    public class SearchQuery
    {
        public SearchQuery() 
        {
            this.Start = string.Empty; 
            this.End = string.Empty;
            this.IsActive = false;
        }
        public int ID { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool IsActive { get; set; }
    }
}
