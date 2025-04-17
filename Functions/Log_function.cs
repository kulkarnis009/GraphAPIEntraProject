using EntraGraphAPI.Constants;
using EntraGraphAPI.Controllers;
using EntraGraphAPI.Data;
using EntraGraphAPI.Models;

namespace EntraGraphAPI.Functions
{
    public class Log_function: BaseApiController
    {
        private readonly DataContext _context;
        public Log_function(DataContext context)
        {
            _context = context;
        }

        public async Task LogAccessDecision(string userId, string appId, string decision, bool? IsXACML, string reason)
        {
            var accessLog = new AccessDecision
            {
                UserId = userId,
                AppId = appId,
                Decision = decision,
                isXACML = IsXACML,
                Timestamp = DateTime.UtcNow,
                Metadata = reason
            };

            await _context.accessDecisions.AddAsync(accessLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogModelDecisions(Evaluation_results evaluation_Results)
        {
            evaluation_Results.test_run_id = formulaConstants.test_run_id;
            
            if(evaluation_Results.model_type?.ToLower() == "hybrid")
            {
                decimal? trust = evaluation_Results.final_trust_factor;
                if (trust == null)
                {
                    evaluation_Results.risk_level = "unknown";
                }
                else if(trust >= 0.70m)
                {
                    evaluation_Results.risk_level = "low";
                }
                else if(trust >= 0.40m)
                {
                    evaluation_Results.risk_level = "medium";
                }
                else
                {
                    evaluation_Results.risk_level = "high";
                }
            }
            else
            {
                evaluation_Results.risk_level = evaluation_Results.final_result.HasValue ? (bool)evaluation_Results.final_result ? "low" : "high" : "high";
            }

            await _context.evaluation_Results.AddAsync(evaluation_Results);
            await _context.SaveChangesAsync();
        }
    }
}