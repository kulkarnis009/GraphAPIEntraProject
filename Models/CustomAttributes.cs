using System.ComponentModel.DataAnnotations;

namespace EntraGreaphAPI.Models
{
        public class CustomAttributes
    {
        [Key]
        public int custom_attribute_id { get; set; }
        [Required]
        public string user_id { get; set; }
        public string? AttributeSet { get; set; }
        [Required]
        public string AttributeName { get; set; }
        [Required]
        public string AttributeValue { get; set; }
        [Required]
        public DateTime LastUpdatedDate { get; set; }
    }

        public class ReceiveCustomAttributes
    {
        [Required]
        public string user_id { get; set; }
        public string? AttributeSet { get; set; }
        [Required]
        public string AttributeName { get; set; }
        [Required]
        public string AttributeValue { get; set; }
        [Required]
        public DateTime LastUpdatedDate { get; set; }
    }
}