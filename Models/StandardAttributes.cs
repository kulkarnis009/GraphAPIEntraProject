using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class StandardAttributes
    {
        [Key]
        public int attribute_id { get; set; }
        [Required]
        public string attribute_name { get; set;}
        public string? description { get; set; }
        public int weight { get; set; }
        public bool isEssential { get; set; }
    }
}