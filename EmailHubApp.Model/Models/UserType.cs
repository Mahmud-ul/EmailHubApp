using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Model.Models
{
    public class UserType
    {
        public UserType()
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
