using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class UsersAttributes
    {
        [Key]
        public int user_attribute_id { get; set;}
        [Required]
        public int user_id { get; set;}
        public string? AttributeName { get; set;}
        public string? AttributeValue { get; set;}
        [Required]
        public bool isCustom { get; set;}
        [Required]
        public DateTime LastUpdatedDate { get; set;}
    }
}