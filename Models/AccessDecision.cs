using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class AccessDecision
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string AppId { get; set; }
        public string Resource { get; set; }
        [Required]
        public string Decision { get; set; }
        public bool? isXACML { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public string? Metadata { get; set; }
    }
}