using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class evaluateNGACResult
    {
        [Required]
        public String id { get; set; }
        public String? givenName { get; set; }
        public String? surname { get; set; }
        public string attribute_value { get; set; }
        [Required]
        public String resource_id { get; set; }
        public string resource_name { get; set; }
        [Required]
        public String permission_name { get; set; }
        public String? description { get; set; }
    }

}