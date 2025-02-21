namespace EntraGraphAPI.Data
{
    public class XACML_data
    {
        public int policyId { get; set; }
        public Dictionary<string, string> attributePairs { get; set; }
    }

    public static class policyStore
    {
        public static readonly List<XACML_data> PolicyData = new()
        {
            new XACML_data
            {
                policyId = 1,
                attributePairs = new Dictionary<string, string>
                {
                    { "Resource", "b0383a20-1483-4bfb-b67f-5dffd4e578b3" },
                    { "Action", "access" },
                    { "Role", "Engineer" }
                }
            }
        };
    }
}
