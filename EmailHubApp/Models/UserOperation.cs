using EmailHubApp.Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailHubApp.Models
{
    public class UserOperation
    {
        public UserOperation()
        {
            this.Name = string.Empty;
            this.UserName = string.Empty;
            this.Email = string.Empty;
            this.Password = string.Empty;
            this.EntryDate = DateTime.Now;
            this.IsActive = false;
            this.TypeID = 0;
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public string Password { get; set; }
        public int TotalSearched { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? MembershipStarted { get; set; }
        public int? MembershipDuration { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("UserType")]
        public int TypeID { get; set; }

        public virtual UserType? UserType { get; set; }
    }
}
