using EntraGraphAPI.Data;
using EntraGraphAPI.Models;

namespace EntraGraphAPI.Functions
{
    public class OutputData
    {
        public bool Result { get; set; }
        public int AttributesMatchedCount { get; set; }
        public int AttributeTotalCount { get; set; }
    }

    public class XACML_Replicate
    {
        public static OutputData ValidateXACMLDotnet(List<getObjectAttributes> getObjectAttributes, List<getSubjectAttributes> getSubjectAttributes,  string permissionName)
        {
            var outputData = new OutputData { AttributesMatchedCount = 0 };

            // Step 1: Get the resource ID from object attributes
            string resourceId = getObjectAttributes.FirstOrDefault()?.resource_id;
            if (resourceId == null) return outputData; // No valid resource found

            // Step 2: Find a policy that matches resource_id & permission_name
            var policy = policyStore.PolicyData
                .FirstOrDefault(p => p.resource_id == resourceId && p.permission_name == permissionName);

            if (policy == null) return outputData; // No matching policy found

            // Step 3: Convert subject attributes to dictionary
            Dictionary<string, string> inputData = getSubjectAttributes.ToDictionary(x => x.attribute_name, x => x.attribute_value);

            // Step 4: Compare policy attributes with subject attributes
            outputData.AttributeTotalCount = policy.attributePairs.Count;
            foreach (var (key, value) in policy.attributePairs)
            {
                if (inputData.TryGetValue(key, out var inputValue) && inputValue == value)
                {
                    outputData.AttributesMatchedCount++;
                }
            }

            // Step 5: Allow access only if all required attributes match
            outputData.Result = outputData.AttributesMatchedCount == outputData.AttributeTotalCount;

            return outputData;
        }
    }
}
