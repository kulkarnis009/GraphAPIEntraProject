namespace EntraGraphAPI.Data
{
    public class XACML_data
    {
        public string resource_id { get; set; } // Unique resource identifier
        public string permission_name { get; set; } // "read", "write", etc.

        public Dictionary<string, string>? subjectAttributePairs { get; set; } // Required subject attributes
        public Dictionary<string, string>? objectAttributePairs { get; set; } // Required object attributes
    }

    public static class policyStore
    {
        public static readonly List<XACML_data> PolicyData = new()
        {
            new XACML_data
            {
                resource_id = "b0383a20-1483-4bfb-b67f-5dffd4e578b3",
                permission_name = "read",

                subjectAttributePairs = new Dictionary<string, string>
                {
                    { "jobTitle", "Engineer" },
                    { "department", "Engineering" }
                },

                objectAttributePairs = new Dictionary<string, string>
                {
                    { "devExperience", "Confidential" },
                    { "riskLevel", "medium" }
                }
            },
            new XACML_data
            {
                resource_id = "b0383a20-1483-4bfb-b67f-5dffd4e578b3",
                permission_name = "write",

                subjectAttributePairs = new Dictionary<string, string>
                {
                    { "jobTitle", "Engineer" },
                    { "department", "Engineering" },
                    { "writeAccess", "enabled"}
                },

                objectAttributePairs = new Dictionary<string, string>
                {
                    { "devExperience", "Confidential" },
                    { "riskLevel", "medium" }
                }
            },
            new XACML_data
            {
                resource_id = "b0383a20-1483-4bfb-b67f-5dffd4e578b3",
                permission_name = "view",

                subjectAttributePairs = new Dictionary<string, string>
                {

                },

                objectAttributePairs = new Dictionary<string, string>
                {
                    { "devExperience", "Confidential" },
                    { "riskLevel", "medium" }
                }
            },
            new XACML_data
            {
                resource_id = "832b229a-5b81-47a0-8b9b-9e67607f841e",
                permission_name = "read",

                subjectAttributePairs = new Dictionary<string, string>
                {

                },

                objectAttributePairs = new Dictionary<string, string>
                {
                    { "department", "sales" },
                    { "riskLevel", "medium" }
                }
            },
            new XACML_data
            {
                resource_id = "832b229a-5b81-47a0-8b9b-9e67607f841e",
                permission_name = "report",

                subjectAttributePairs = new Dictionary<string, string>
                {
                    { "jobTitle", "Sales Associate" },
                    { "department", "Sales" },
                    {"officeLocation", "Prince george"}
                },

                objectAttributePairs = new Dictionary<string, string>
                {
                    { "department", "sales" },
                    { "riskLevel", "high" }
                }
            }
        };
    }
}
