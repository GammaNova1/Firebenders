using System.ComponentModel.DataAnnotations;

namespace Firebenders.Models
{
    public class Records 
    {
        [Key]
        public int RecordId { get; set; }
        public string RecordName { get; set; }
        public string RecordImage { get; set; }
        public string RecordLatitude { get; set; }
        public string RecordLongtitude { get; set; }
        public bool RecordStatus { get; set; }
        public DateTime RecordDate { get; set; }

    }
}
