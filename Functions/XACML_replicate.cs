using EntraGraphAPI.Data;

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
        public static OutputData ValidateXACMLDotnet(XACML_data inputData)
        {
            var outputData = new OutputData { AttributesMatchedCount = 0 };

            // Find matching policy
            var policy = policyStore.PolicyData.FirstOrDefault(x => x.policyId == inputData.policyId);
            if (policy == null) return outputData; // Return default if policy doesn't exist

            // Count total attributes in policy
            outputData.AttributeTotalCount = policy.attributePairs.Count;

            // Check how many attributes match
            foreach (var (key, value) in policy.attributePairs)
            {
                if (inputData.attributePairs.TryGetValue(key, out var inputValue) && inputValue == value)
                {
                    outputData.AttributesMatchedCount++;
                }
            }

            // Decision based on match count
            outputData.Result = outputData.AttributesMatchedCount == outputData.AttributeTotalCount;

            return outputData;
        }
    }
}
