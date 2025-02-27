namespace EntraGraphAPI.Data
{
    public class XACML_data
    {
        public string resource_id { get; set; } // Unique resource identifier
        public string permission_name { get; set; } // "read", "write", etc.
        public Dictionary<string, string> attributePairs { get; set; } // Required subject attributes
    }

    public static class policyStore
    {
        public static readonly List<XACML_data> PolicyData = new()
        {
            new XACML_data
            {
                resource_id = "b0383a20-1483-4bfb-b67f-5dffd4e578b3",
                permission_name = "read",
                attributePairs = new Dictionary<string, string>
                {
                    { "jobTitle", "Engineer" },  // User must be an "Engineer"
                    { "riskLevel", "medium" }  // User must belong to "Engineering"
                }
            },
            new XACML_data
            {
                resource_id = "b0383a20-1483-4bfb-b67f-5dffd4e578b3",
                permission_name = "write",
                attributePairs = new Dictionary<string, string>
                {
                    { "jobTitle", "Senior Engineer" },
                    { "clearanceLevel", "High" }
                }
            }
        };
    }
}
