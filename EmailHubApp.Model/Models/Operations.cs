using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Model.Models
{
    public class Operations
    {
        public Operations() 
        {
            this.ActionName = string.Empty;
            this.Description = string.Empty;
            this.ActionTime = DateTime.MinValue;          
            this.ActionUser = 0;
        }
        public int ID { get; set; }
        public string ActionName { get; set; }
        public string Description { get; set; }
        public DateTime ActionTime { get; set; }

        [ForeignKey("User")]
        public int ActionUser { get; set; }

        public virtual User User { get; set; }
    }
}
