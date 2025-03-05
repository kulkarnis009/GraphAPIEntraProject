using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EntraGraphAPI.Data
{
public class XACML_structure
{
    public string resource_id { get; set; }
    public string permission_name { get; set; }
    public Dictionary<string, string>? subjectAttributePairs { get; set; } // String-based matches
    public Dictionary<string, NumericCondition>? subjectNumericConditions { get; set; } // Numeric ranges
    public Dictionary<string, string>? objectAttributePairs { get; set; } 
    public Dictionary<string, NumericCondition>? objectNumericConditions { get; set; } 
}

public class NumericCondition
{
    public double? Min { get; set; } // Minimum allowed value
    public double? Max { get; set; } // Maximum allowed value
}


    public static class policyStore
    {
        private static readonly string PolicyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "XACML_policies.json");

        public static List<XACML_structure> PolicyData { get; private set; } = new();

        static policyStore()
        {
            LoadPolicies();
        }

        private static void LoadPolicies()
        {
            if (File.Exists(PolicyFilePath))
            {
                string json = File.ReadAllText(PolicyFilePath);
                PolicyData = JsonSerializer.Deserialize<List<XACML_structure>>(json) ?? new List<XACML_structure>();
                System.Console.WriteLine(PolicyData[0]);
            }
        }
    }
}
