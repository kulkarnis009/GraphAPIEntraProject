using EntraGraphAPI.Data;
using EntraGraphAPI.Models;

namespace EntraGraphAPI.Functions
{
    public class OutputData
{
    public bool Result { get; set; }
    public int SubjectWeightedScore { get; set; }
    public int ObjectWeightedScore { get; set; }
    public int SubjectTotalWeight { get; set; }
    public int ObjectTotalWeight { get; set; }
    public double XacmlTrustFactor { get; set; } // New trust factor metric
}

    public class XACML_Replicate
    {
        public static OutputData ValidateXACMLDotnet(List<getObjectAttributes> getObjectAttributes, List<getSubjectAttributes> getSubjectAttributes,  string permissionName)
        {
            var outputData = new OutputData 
            { 
                SubjectWeightedScore = 0, 
                ObjectWeightedScore = 0, 
                XacmlTrustFactor = 0 
            };

            // Step 1: Get the resource ID from object attributes
            string resourceId = getObjectAttributes.FirstOrDefault()?.resource_id;
            if (resourceId == null) return outputData; // No valid resource found

            // Step 2: Find a policy that matches resource_id & permission_name
            var policy = policyStore.PolicyData
                .FirstOrDefault(p => p.resource_id == resourceId && p.permission_name == permissionName);

            if (policy == null) return outputData; // No matching policy found

            // Step 3: Convert subject attributes to dictionary (key = name, value = (actualValue, weight))
            Dictionary<string, (string value, int weight)> inputData = getSubjectAttributes
                .ToDictionary(x => x.attribute_name, x => (x.attribute_value, x.weight));

            // Step 4: Convert object attributes to dictionary
            Dictionary<string, (string value, int weight)> objectData = getObjectAttributes
                .ToDictionary(x => x.attribute_name, x => (x.attribute_value, x.weight));

            // Step 5: Compare subject attributes and calculate weighted score
            foreach (var (key, requiredValue) in policy.subjectAttributePairs)
            {
                if (inputData.TryGetValue(key, out var subjectData) && subjectData.value == requiredValue)
                {
                    outputData.SubjectWeightedScore += subjectData.weight; // Add weight if matched
                }
                outputData.SubjectTotalWeight += subjectData.weight; // Total weight of subject attributes
            }

            // Step 6: Compare object attributes and calculate weighted score
            foreach (var (key, requiredValue) in policy.objectAttributePairs)
            {
                if (objectData.TryGetValue(key, out var objectAttr) && objectAttr.value == requiredValue)
                {
                    outputData.ObjectWeightedScore += objectAttr.weight; // Add weight if matched
                }
                outputData.ObjectTotalWeight += objectAttr.weight; // Total weight of object attributes
            }

            // Step 7: Calculate XACML Trust Factor
            double totalWeightedScore = outputData.SubjectWeightedScore + outputData.ObjectWeightedScore;
            double totalPossibleWeight = outputData.SubjectTotalWeight + outputData.ObjectTotalWeight;
            
            outputData.XacmlTrustFactor = (totalPossibleWeight > 0) ? totalWeightedScore / totalPossibleWeight : 0;

            // Step 8: Grant access only if ALL required attributes (subject + object) match
            outputData.Result = totalWeightedScore == totalPossibleWeight;

            return outputData;
        }
    }
}
