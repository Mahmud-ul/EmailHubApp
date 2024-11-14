using System.ComponentModel.DataAnnotations.Schema;

namespace EmailHubApp.Models
{
    public class SearchedDataOperation
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string SearchedKey { get; set; }
        public string SearchedValue { get; set; }
        public DateTime SearchDate { get; set; }


    }
}
