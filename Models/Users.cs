using System.ComponentModel.DataAnnotations;

namespace EntraGreaphAPI.Models
{
    public class Users
    {
        [Key]
        public int user_id { get; set;}
        [Key]
        public string user_UUID { get; set;}
        [Required]
        public string user_fisrt_name { get; set;}
        [Required]
        public string user_last_name { get; set;}
        [Required]
        public string user_email_id { get; set;}

    }
}