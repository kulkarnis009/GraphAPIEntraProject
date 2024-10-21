namespace EntraGraphAPI.Models
{
    public class Policy
    {
        public string Subject { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public bool Access { get; set; }
    }
}