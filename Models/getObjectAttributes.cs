namespace EntraGraphAPI.Models
{
    public class getObjectAttributes
    {
        public int? permission_id { get; set; }
        public string? resource_id { get; set; }
        public string? permission_name { get; set; }
        public string? attribute_name { get; set; }
        public int weight { get; set; }
        public string? attribute_value { get; set; }
    }
}