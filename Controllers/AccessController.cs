using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AutoMapper;
using EntraGraphAPI.Constants;
using EntraGraphAPI.Data;
using EntraGraphAPI.Functions;
using EntraGraphAPI.Models;
using EntraGraphAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Controllers
{
    public class AccessController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly UsersController _usersController;
        private readonly ApplicationController _applicationController;
        private readonly XacmlPdpService _xacmlPdpService;
        public AccessController(DataContext context, IMapper _mapper, GraphApiService _graphApiService, XacmlPdpService xacmlPdpService)
        {
            _context = context;
            _usersController = new UsersController(_graphApiService, _context, _mapper);
            _applicationController = new ApplicationController(_graphApiService, _context, _mapper);
            _xacmlPdpService = xacmlPdpService;
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize([FromForm] string id_token)
        {
            if (string.IsNullOrEmpty(id_token))
            {
                return BadRequest(new { error = "The id_token field is required." });
            }
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(id_token);

            // Extract User Object ID (oid)
            var userId = token.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
            // Extract App ID (aud)
            var appId = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(appId))
            {
                return BadRequest(new { error = "User ID or App ID is missing." });
            }
            // print token information
            System.Console.WriteLine("User Id : ", userId);
            System.Console.WriteLine("App Client Id : ", appId);

            // refreshing user attributes
            await _usersController.GetSingleUserbyUUID(userId);
            System.Console.WriteLine("got user attributes");

            await _usersController.GetUserDetailsCust(userId);
            System.Console.WriteLine("got user custom attributes");

            await _usersController.getLogs(userId,appId,750);
            System.Console.WriteLine("got log attributes");

            // Evaluating access with NGAC and XACML engine
            var getNGACAccess = await evaluateNGACAccess(userId, appId);


            if (getNGACAccess == null)
            {
                await LogAccessDecision(userId, appId, "Deny", false, "NGAC evaluation failed.");
                // Return an HTML page for "Access Denied by NGAC"
                return Content(htmlResponses.denyResponse, "text/html");
            }

            var responseXml = await _xacmlPdpService.EvaluatePolicyAsync(getNGACAccess.attribute_value, getNGACAccess.resource_name, getNGACAccess.permission_name);

            // Parse the decision from the response
            var getXACMLAccess = XACML_functions.ParseDecision(responseXml);

            if (getXACMLAccess == null || getXACMLAccess != "Permit")
            {
                await LogAccessDecision(userId, appId, "Deny", true, "XACML evaluation failed.");
                // Return an HTML page for "Access Denied by XACML"
                return Content(htmlResponses.denyResponseXACML, "text/html");
            }


            List<String>? getRedirect = await _applicationController.GetReplyUrlsByClientIdAsync(appId);
            // Return an HTML page to display the information

            await LogAccessDecision(userId, appId, "Permit", null, "Authorize success.");
            return Content(htmlResponses.getSuccessResponse(getNGACAccess,getXACMLAccess, getRedirect), "text/html");

        }

        private async Task<evaluateNGACResult?> evaluateNGACAccess(string userId, string appId)
        {
            var getAccessResult = await _context.evaluateAccessResults.FromSqlInterpolated($"Select * from evaluateAccess({userId}, {appId})").FirstOrDefaultAsync();

            if (getAccessResult == null) return null;
            System.Console.WriteLine(getAccessResult.attribute_value + getAccessResult.resource_name);
            return getAccessResult;
        }

        [HttpPost("evaluateXACML")]
        public async Task<IActionResult> EvaluateXACMLAccess([FromBody] XACMLAccessRequest request)
        {
            try
            {
                // Call the PDP service
                var responseXml = await _xacmlPdpService.EvaluatePolicyAsync(request.Role, request.Resource, request.Action);

                // Parse the decision from the response
                var decision = XACML_functions.ParseDecision(responseXml);

                return Ok(new { decision });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task LogAccessDecision(string userId, string appId, string decision, bool? IsXACML, string reason)
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
    }
}