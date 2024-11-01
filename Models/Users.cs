using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class Users
    {
        [Key]
        public int user_id { get; set;}
        [Key]
        public string id { get; set;}
        public string? givenName { get; set;}
        public string? surname { get; set;}
        // public string? mail { get; set;}
        // public string? userPrincipalName { get; set;}
        // public string? jobTitle { get; set;}
        // public string? officeLocation { get; set;}
        // public string? preferredLanguage { get; set;}
        // public DateTime? LastUpdatedDate { get; set;}
    }

        public class RecieveUsers
    {

        [Required]
        public string id { get; set;}
        public string? givenName { get; set;}
        public string? surname { get; set;}
        // public string? mail { get; set;}
        // public string? userPrincipalName { get; set;}
        // public string? jobTitle { get; set;}
        // public string? officeLocation { get; set;}
        // public string? preferredLanguage { get; set;}

    }
}