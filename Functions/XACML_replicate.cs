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
        public double XacmlTrustFactor { get; set; } // Trust factor metric
        public int UnmatchedEssentialCount { get; set; } // Number of essential attributes that were missing or mismatched
    }

    public class XACML_Replicate
    {
        private const double TrustThreshold = 0.7; // Minimum required trust factor (70%)

        public static OutputData ValidateXACMLDotnet(
        List<getObjectAttributes> getObjectAttributes, 
        List<getSubjectAttributes> getSubjectAttributes, 
        string permissionName,
        Dictionary<string, (int weight, bool isEssential)> standardAttributes)
    {
        var outputData = new OutputData 
        { 
            SubjectWeightedScore = 0, 
            ObjectWeightedScore = 0, 
            XacmlTrustFactor = 0,
            UnmatchedEssentialCount = 0
        };

        // Step 1: Get the resource ID from object attributes
        string resourceId = getObjectAttributes.FirstOrDefault()?.resource_id;
        if (resourceId == null) return outputData; // No valid resource found

        // Step 2: Find a policy that matches resource_id & permission_name
        var policy = policyStore.PolicyData
            .FirstOrDefault(p => p.resource_id == resourceId && p.permission_name == permissionName);

        if (policy == null) return outputData; // No matching policy found

        // Step 3: Convert object attributes to dictionary
        Dictionary<string, string> objectData = getObjectAttributes
            .ToDictionary(x => x.attribute_name, x => x.attribute_value);

        // Step 4: Convert subject attributes to dictionary
        Dictionary<string, string> subjectData = getSubjectAttributes
            .ToDictionary(x => x.attribute_name, x => x.attribute_value);

        // Step 5: Check if all essential attributes exist and match
        foreach (var (attrName, requiredValue) in policy.subjectAttributePairs)
        {
            if (standardAttributes.TryGetValue(attrName, out var attrInfo) && attrInfo.isEssential)
            {
                if (!subjectData.TryGetValue(attrName, out var actualValue) || actualValue != requiredValue)
                {
                    // Essential attribute missing or mismatched → Deny access immediately
                    outputData.UnmatchedEssentialCount++;
                }
            }
        }

        // If any essential attributes are missing or mismatched, deny access
        if (outputData.UnmatchedEssentialCount > 0)
        {
            outputData.Result = false;
            outputData.XacmlTrustFactor = 0;
            return outputData;
        }

        // Step 6: Compare subject attributes and calculate weighted score
        foreach (var (attrName, requiredValue) in policy.subjectAttributePairs)
        {
            if (subjectData.TryGetValue(attrName, out var actualValue) && actualValue == requiredValue)
            {
                if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                {
                    outputData.SubjectWeightedScore += attrInfo.weight; // Add weight if matched
                }
            }
            if (standardAttributes.TryGetValue(attrName, out var attrWeight))
            {
                outputData.SubjectTotalWeight += attrWeight.weight; // Total weight of subject attributes
            }
        }

        // Step 7: Compare object attributes and calculate weighted score
        foreach (var (attrName, requiredValue) in policy.objectAttributePairs)
        {
            if (objectData.TryGetValue(attrName, out var actualValue) && actualValue == requiredValue)
            {
                if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                {
                    outputData.ObjectWeightedScore += attrInfo.weight; // Add weight if matched
                }
            }
            if (standardAttributes.TryGetValue(attrName, out var attrWeight))
            {
                outputData.ObjectTotalWeight += attrWeight.weight; // Total weight of object attributes
            }
        }

        // Step 8: Calculate XACML Trust Factor
        double totalWeightedScore = outputData.SubjectWeightedScore + outputData.ObjectWeightedScore;
        double totalPossibleWeight = outputData.SubjectTotalWeight + outputData.ObjectTotalWeight;

        outputData.XacmlTrustFactor = (totalPossibleWeight > 0) ? totalWeightedScore / totalPossibleWeight : 0;

        // Step 9: Grant access only if the trust factor is >= 70%
        outputData.Result = outputData.XacmlTrustFactor >= TrustThreshold;

        return outputData;
    }

        public static bool ValidateXACMLSimple(
        List<getObjectAttributes> getObjectAttributes, 
        List<getSubjectAttributes> getSubjectAttributes, 
        string permissionName)
        {
            // Step 1: Get the resource ID from object attributes
            string resourceId = getObjectAttributes.FirstOrDefault()?.resource_id;
            if (resourceId == null) return false; // No valid resource found → Deny

            // Step 2: Find a policy that matches resource_id & permission_name
            var policy = policyStore.PolicyData
                .FirstOrDefault(p => p.resource_id == resourceId && p.permission_name == permissionName);

            if (policy == null) return false; // No matching policy found → Deny

            // Step 3: Convert object attributes to dictionary
            Dictionary<string, string> objectData = getObjectAttributes
                .ToDictionary(x => x.attribute_name, x => x.attribute_value);

            // Step 4: Convert subject attributes to dictionary
            Dictionary<string, string> subjectData = getSubjectAttributes
                .ToDictionary(x => x.attribute_name, x => x.attribute_value);

            // Step 5: Check if all subject attributes match
            foreach (var (attrName, requiredValue) in policy.subjectAttributePairs)
            {
                if (!subjectData.TryGetValue(attrName, out var actualValue) || actualValue != requiredValue)
                {
                    return false; // Subject attribute mismatch → Deny
                }
            }

            // Step 6: Check if all object attributes match
            foreach (var (attrName, requiredValue) in policy.objectAttributePairs)
            {
                if (!objectData.TryGetValue(attrName, out var actualValue) || actualValue != requiredValue)
                {
                    return false; // Object attribute mismatch → Deny
                }
            }

            // If all attributes match → Permit
            return true;
        }

    }
}
