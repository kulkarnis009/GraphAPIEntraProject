using System.ComponentModel.DataAnnotations;

namespace EntraGraphAPI.Models
{
    public class Scenarios
    {
        [Key]
        public int scenario_id { get; set; }  // Primary Key
        public string resource_id { get; set; }
        public string user_id { get; set; }
        public string permission_name { get; set; }
    }

    public class Evaluation_results
    {
        [Key]
        public int result_id { get; set; }  // Primary Key
        public int? scenario_id { get; set; }  // Foreign Key
        public string? model_type { get; set; }
        public DateTime? result_date { get; set; }
        public float? xacml_threshold { get; set; }
        public float? xacml_constant { get; set; }
        public float? ngac_constant { get; set; }
        public bool? xacml_result { get; set; }
        public int? subjectWeightedScore { get; set; }
        public int? subjectTotalWeight { get; set; }
        public int? objectWeightedScore { get; set; }
        public int? objectTotalWeight { get; set; }
        public float? xacmlTrustFactor { get; set; }
        public int? unmatchedEssentialCount { get; set; }
        public float? ngacTrustFactor { get; set; }
        public int? denyCount { get; set; }
        public int? denyThreshold { get; set; }
        public int? permitCount { get; set; }
        public int? accessCount { get; set; }
        public float? final_trust_factor { get; set; }
        public bool? final_result { get; set; }
        public int? test_run_id { get; set; }
        public string? risk_level { get; set; }
    }
}