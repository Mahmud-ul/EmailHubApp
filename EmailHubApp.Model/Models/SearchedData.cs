using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Model.Models
{
    public class SearchedData
    {
        public SearchedData()
        {
            this.UserID = 0;
            this.SearchedKey = string.Empty;
            this.SearchedValue = string.Empty;
            this.SearchDate = DateTime.Today;
        }
        public int ID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public string SearchedKey { get; set; }
        public string SearchedValue { get; set; }
        public DateTime SearchDate { get; set; }

        public virtual User User { get; set; }
    }
}
