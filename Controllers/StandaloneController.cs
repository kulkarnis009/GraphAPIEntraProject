using EntraGraphAPI.Data;
using EntraGraphAPI.Functions;
using EntraGraphAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Controllers
{
    public class StandaloneController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly UsersController _usersController;
        private readonly Log_function _logFunction;

        public StandaloneController(DataContext context, UsersController usersController, Log_function logFunction)
        {
            _context = context;
            _usersController = usersController;
            _logFunction = logFunction;
        }

        //   Standalone NGAC and XACML
        private async Task<newEvaluateNGACResult?> evaluateNewNGACAccess(string userId, string appId)
        {
            var getAccessResult = await _context.newEvaluateAccessResults.FromSqlInterpolated($"Select * from newEvaluateNGACaccess({userId}, {appId})").FirstOrDefaultAsync();

            if (getAccessResult == null) return null;
            return getAccessResult;
        }

        [HttpPost("newAuthorize/{userId}/{appId}/{permission_name}")]
        public async Task<ActionResult> newAccess(string userId, string appId, string permission_name)
        {
            double totalTrust = 0;
            OutputData responseXml = null;
            // refreshing user attributes
            await _usersController.GetSingleUserbyUUID(userId);
            System.Console.WriteLine("got user attributes");

            await _usersController.GetUserDetailsCust(userId);
            System.Console.WriteLine("got user custom attributes");

            await _usersController.getLogs(userId,appId,750);
            System.Console.WriteLine("got log attributes");

            var objectAttributes = await _context.getObjectAttributes.FromSqlInterpolated($"Select * from getObjectAttributes({appId})").ToListAsync();

            if(objectAttributes != null)
            {

                var usersAttributes = await _context.getsubjectAttributes.FromSqlInterpolated($"Select * from getSubjectAttributes({userId})").ToListAsync();
            var getNGACAccess = await evaluateNewNGACAccess(userId, appId);

            if(getNGACAccess == null)
            {
                await _logFunction.LogAccessDecision(userId, appId, "Deny", false, "NGAC evaluation failed.");
            }


            var standard_attributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => (sa.weight, sa.isEssential));

            responseXml = XACML_Replicate.ValidateXACMLDotnet(objectAttributes, usersAttributes, permission_name, standard_attributes);
                
            if(responseXml.Result == false)
            {
                await _logFunction.LogAccessDecision(userId, appId, "Deny", true, "XACML evaluation failed.");
            }

            totalTrust = ((getNGACAccess.trustFactor * 100) + responseXml.XacmlTrustFactor) / 2;
            await _logFunction.LogAccessDecision(userId, appId, "Permit", null, "Authorize success.");

            }
            return Ok(totalTrust);
        }
    }
}