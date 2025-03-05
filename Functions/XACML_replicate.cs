using EntraGraphAPI.Constants;
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
                SubjectTotalWeight = 0,
                ObjectTotalWeight = 0,
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

            // Step 3: Convert object & subject attributes to dictionaries
            Dictionary<string, string> objectData = getObjectAttributes
                .ToDictionary(x => x.attribute_name, x => x.attribute_value);
            
            Dictionary<string, string> subjectData = getSubjectAttributes
                .ToDictionary(x => x.attribute_name, x => x.attribute_value);

            // Step 4: Evaluate subject attributes (String & Numeric)
            foreach (var (attrName, requiredValue) in policy.subjectAttributePairs ?? new Dictionary<string, string>())
            {
                bool isMatched = subjectData.TryGetValue(attrName, out var actualValue) && actualValue == requiredValue;

                if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                {
                    outputData.SubjectTotalWeight += attrInfo.weight; // Always add total weight

                    if (isMatched)
                    {
                        outputData.SubjectWeightedScore += attrInfo.weight; //  Matched → Add weighted score
                    }
                    else if (attrInfo.isEssential)
                    {
                        outputData.UnmatchedEssentialCount++; //  Essential but missing/mismatched
                    }
                }
            }

            // Step 5: Evaluate numeric subject attributes (Range Matching)
            foreach (var (attrName, condition) in policy.subjectNumericConditions ?? new Dictionary<string, NumericCondition>())
            {
                if (subjectData.TryGetValue(attrName, out var actualValueStr) && double.TryParse(actualValueStr, out double actualValue))
                {
                    bool isInRange = (!condition.Min.HasValue || actualValue >= condition.Min.Value) &&
                                    (!condition.Max.HasValue || actualValue <= condition.Max.Value);

                    if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                    {
                        outputData.SubjectTotalWeight += attrInfo.weight; //  Always add total weight

                        if (isInRange)
                        {
                            outputData.SubjectWeightedScore += attrInfo.weight; //  In range → Add weighted score
                        }
                        else if (attrInfo.isEssential)
                        {
                            outputData.UnmatchedEssentialCount++; //  Essential but out of range
                        }
                    }
                }
                else if (standardAttributes.TryGetValue(attrName, out var attrInfo) && attrInfo.isEssential)
                {
                    outputData.UnmatchedEssentialCount++; //  Essential numeric attribute missing
                }
            }

            // Step 6: Evaluate object attributes (String & Numeric)
            foreach (var (attrName, requiredValue) in policy.objectAttributePairs ?? new Dictionary<string, string>())
            {
                bool isMatched = objectData.TryGetValue(attrName, out var actualValue) && actualValue == requiredValue;

                if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                {
                    outputData.ObjectTotalWeight += attrInfo.weight; // Always add total weight

                    if (isMatched)
                    {
                        outputData.ObjectWeightedScore += attrInfo.weight; //  Matched → Add weighted score
                    }
                    else if (attrInfo.isEssential)
                    {
                        outputData.UnmatchedEssentialCount++; //  Essential but missing/mismatched
                    }
                }
            }

            // Step 7: Evaluate numeric object attributes (Range Matching)
            foreach (var (attrName, condition) in policy.objectNumericConditions ?? new Dictionary<string, NumericCondition>())
            {
                if (objectData.TryGetValue(attrName, out var actualValueStr) && double.TryParse(actualValueStr, out double actualValue))
                {
                    bool isInRange = (!condition.Min.HasValue || actualValue >= condition.Min.Value) &&
                                    (!condition.Max.HasValue || actualValue <= condition.Max.Value);

                    if (standardAttributes.TryGetValue(attrName, out var attrInfo))
                    {
                        outputData.ObjectTotalWeight += attrInfo.weight; //  Always add total weight

                        if (isInRange)
                        {
                            outputData.ObjectWeightedScore += attrInfo.weight; //  In range → Add weighted score
                        }
                        else if (attrInfo.isEssential)
                        {
                            outputData.UnmatchedEssentialCount++; //  Essential but out of range
                        }
                    }
                }
                else if (standardAttributes.TryGetValue(attrName, out var attrInfo) && attrInfo.isEssential)
                {
                    outputData.UnmatchedEssentialCount++; //  Essential numeric attribute missing
                }
            }

            // Step 8: If any essential attributes are unmatched, deny access
            if (outputData.UnmatchedEssentialCount > 0)
            {
                outputData.Result = false;
                outputData.XacmlTrustFactor = 0;
                return outputData;
            }

            // Step 9: Calculate XACML Trust Factor
            double totalWeightedScore = outputData.SubjectWeightedScore + outputData.ObjectWeightedScore;
            double totalPossibleWeight = outputData.SubjectTotalWeight + outputData.ObjectTotalWeight;

            outputData.XacmlTrustFactor = (totalPossibleWeight > 0) ? totalWeightedScore / totalPossibleWeight : 0;

            // Step 10: Grant access only if the trust factor is >= 70%
            outputData.Result = outputData.XacmlTrustFactor >= formulaConstants.xacmlTrustThreshold;

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
            foreach (var (attrName, requiredValue) in policy.subjectAttributePairs ?? new Dictionary<string, string>())
            {
                if (!subjectData.TryGetValue(attrName, out var actualValue) || actualValue != requiredValue)
                {
                    return false; // Subject attribute mismatch → Deny
                }
            }

            // Step 6: Check if all subject numeric attributes fall within range
            foreach (var (attrName, condition) in policy.subjectNumericConditions ?? new Dictionary<string, NumericCondition>())
            {
                if (subjectData.TryGetValue(attrName, out var actualValueStr) && double.TryParse(actualValueStr, out double actualValue))
                {
                    bool isInRange = (!condition.Min.HasValue || actualValue >= condition.Min.Value) &&
                                    (!condition.Max.HasValue || actualValue <= condition.Max.Value);

                    if (!isInRange)
                    {
                        return false; // Subject numeric attribute out of range → Deny
                    }
                }
                else
                {
                    return false; // Subject numeric attribute missing or invalid → Deny
                }
            }

            // Step 7: Check if all object attributes match
            foreach (var (attrName, requiredValue) in policy.objectAttributePairs ?? new Dictionary<string, string>())
            {
                if (!objectData.TryGetValue(attrName, out var actualValue) || actualValue != requiredValue)
                {
                    return false; // Object attribute mismatch → Deny
                }
            }

            // Step 8: Check if all object numeric attributes fall within range
            foreach (var (attrName, condition) in policy.objectNumericConditions ?? new Dictionary<string, NumericCondition>())
            {
                if (objectData.TryGetValue(attrName, out var actualValueStr) && double.TryParse(actualValueStr, out double actualValue))
                {
                    bool isInRange = (!condition.Min.HasValue || actualValue >= condition.Min.Value) &&
                                    (!condition.Max.HasValue || actualValue <= condition.Max.Value);

                    if (!isInRange)
                    {
                        return false; // Object numeric attribute out of range → Deny
                    }
                }
                else
                {
                    return false; // Object numeric attribute missing or invalid → Deny
                }
            }

            // If all attributes match (including numeric ranges) → Permit
            return true;
        }
    }
}
