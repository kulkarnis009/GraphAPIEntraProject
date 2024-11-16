using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Dto
{
    public class ReceiveCustomAttributes
    {
        [Required]
        public int user_id { get; set; }
        [Required]
        public string id { get; set; }
        // public string? AttributeSet { get; set; }
        [Required]
        public string AttributeName { get; set; }
        [Required]
        public string attribute_value { get; set; }
        [Required]
        public DateTime last_updated_date { get; set; }
    }
}