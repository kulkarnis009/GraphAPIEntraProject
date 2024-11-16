using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class UsersAttributes
    {
        [Key]
        public int user_attribute_id { get; set;}
        [Required]
        public int user_id { get; set;}
        [Required]
        public int attribute_id { get; set;}
        [Required]
        public string attribute_value { get; set;}
        [Required]
        public bool is_custom { get; set;}
        [Required]
        public DateTime last_updated_date { get; set;}
    }
}