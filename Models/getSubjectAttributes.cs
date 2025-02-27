namespace EntraGraphAPI.Models
{
    public class getSubjectAttributes
    {
        public int user_id { get; set; }
        public string id { get; set; }
        public int attribute_id { get; set; }
        public string attribute_name { get; set; }
        public int weight { get; set; }
        public bool isEssential { get; set; }
        public string attribute_value { get; set; }
    }
}