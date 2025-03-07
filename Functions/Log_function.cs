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
            await _context.evaluation_Results.AddAsync(evaluation_Results);
            await _context.SaveChangesAsync();
        }
    }
}