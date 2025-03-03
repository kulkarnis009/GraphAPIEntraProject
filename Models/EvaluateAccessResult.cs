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
        public int denyCount { get; set; }
    }

    public class newEvaluateNGACResult
    {
        [Required]
        public string id { get; set; }
        [Required]
        public int user_id { get; set; }
        public String? givenName { get; set; }
        public String? surname { get; set; }
        [Required]
        public String resource_id { get; set; }
        public string? resource_name { get; set; }
        [Required]
        public String permission_name { get; set; }
        public int denyCount {get; set; }
        public int accessCount {get; set; }
        public DateTime firstAccessTime { get; set; }
        public DateTime lastAccessTime { get; set; }
        public int matchedAttributes { get; set; }
        public int totalAttributes { get; set; }
        public double trustFactor { get; set; }

    }

    public class hybridNGAC
    {
        [Required]
        public string id { get; set; }
        [Required]
        public int user_id { get; set; }
        public String? givenName { get; set; }
        public String? surname { get; set; }
        [Required]
        public String resource_id { get; set; }
        public string? resource_name { get; set; }
        [Required]
        public String permission_name { get; set; }
        public int denyCount {get; set; }
        public int denyThreshold {get; set; }
        public int permitCount {get; set; }
        public int accessCount {get; set; }
        public DateTime? firstAccessTime { get; set; }
        public DateTime? lastAccessTime { get; set; }

    }

    public class hybridFinalNGAC : hybridNGAC
    {
        public double NGACTrustFactor { get; set; }
    }

}