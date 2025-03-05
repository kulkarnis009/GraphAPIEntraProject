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
        private readonly Log_function _logFunction;
        private readonly IMapper _mapper;
        public AccessController(DataContext context, IMapper mapper, GraphApiService _graphApiService, XacmlPdpService xacmlPdpService)
        {
            _context = context;
            _usersController = new UsersController(_graphApiService, _context, mapper);
            _applicationController = new ApplicationController(_graphApiService, _context, mapper);
            _xacmlPdpService = xacmlPdpService;
            _logFunction = new Log_function(_context);
            _mapper = mapper;
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

            await _usersController.getLogs(userId, appId, 750);
            System.Console.WriteLine("got log attributes");

            // Evaluating access with NGAC and XACML engine
            var getNGACAccess = await evaluateNGACAccess(userId, appId);


            if (getNGACAccess == null)
            {
                await _logFunction.LogAccessDecision(userId, appId, "Deny", false, "NGAC evaluation failed.");
                // Return an HTML page for "Access Denied by NGAC"
                return Content(htmlResponses.denyResponse, "text/html");
            }

            var responseXml = await _xacmlPdpService.EvaluatePolicyAsync(getNGACAccess.attribute_value, getNGACAccess.resource_name, getNGACAccess.permission_name);

            // Parse the decision from the response
            var getXACMLAccess = XACML_functions.ParseDecision(responseXml);

            if (getXACMLAccess == null || getXACMLAccess != "Permit")
            {
                await _logFunction.LogAccessDecision(userId, appId, "Deny", true, "XACML evaluation failed.");
                // Return an HTML page for "Access Denied by XACML"
                return Content(htmlResponses.denyResponseXACML, "text/html");
            }


            List<String>? getRedirect = await _applicationController.GetReplyUrlsByClientIdAsync(appId);
            // Return an HTML page to display the information

            await _logFunction.LogAccessDecision(userId, appId, "Permit", null, "Authorize success.");
            return Content(htmlResponses.getSuccessResponse(getNGACAccess, getXACMLAccess, getRedirect), "text/html");

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


        // Hybrid NGAC and XACML
        private async Task<hybridNGAC?> evaluateHybridNGACAccess(string userId, string appId, string permissionName)
        {
            var getAccessResult = await _context.hybridNGACs.FromSqlInterpolated($"Select * from hybridNGAC({userId}, {appId}, {permissionName})").FirstOrDefaultAsync();

            if (getAccessResult == null) return null;
            return getAccessResult;
        }

        [HttpPost("hybrid/{scenarioId}/{userId}/{appId}/{permission_name}")]
        public async Task<ActionResult> hybridAccess(int scenarioId, string userId, string appId, string permission_name)
        {
            // var scenario = await _context.scenarios.Where(x => x.scenario_id == scenarioId).FirstOrDefaultAsync();
            
            // string userId = scenario.user_id;
            // string appId = scenario.resource_id;
            // string permission_name = scenario.permission_name;

            double totalTrust = 0;
            OutputData responseXml = null;
            hybridFinalNGAC getNGACAccess = null;

            // refreshing user attributes
            await _usersController.GetSingleUserbyUUID(userId);
            System.Console.WriteLine("got user attributes");

            await _usersController.GetUserDetailsCust(userId);
            System.Console.WriteLine("got user custom attributes");

            // await _usersController.getLogs(userId,appId,750);
            // System.Console.WriteLine("got log attributes");

            var objectAttributes = await _context.getObjectAttributes.FromSqlInterpolated($"Select * from getObjectAttributes({appId}, {permission_name})").ToListAsync();

            if (objectAttributes != null)
            {

                var usersAttributes = await _context.getsubjectAttributes.FromSqlInterpolated($"Select * from getSubjectAttributes({userId})").ToListAsync();

                if (usersAttributes != null)
                {

                    var standard_attributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => (sa.weight, sa.isEssential));

                    responseXml = XACML_Replicate.ValidateXACMLDotnet(objectAttributes, usersAttributes, permission_name, standard_attributes);

                    if (responseXml.Result == false)
                    {
                        await _logFunction.LogModelDecisions(new Evaluation_results
                        {
                            scenario_id = scenarioId,
                            model_type = "Hybrid",
                            result_date = DateTime.Now,
                            xacml_result = false,
                            subjectWeightedScore = responseXml.SubjectWeightedScore,
                            subjectTotalWeight = responseXml.SubjectTotalWeight,
                            objectWeightedScore = responseXml.ObjectWeightedScore,
                            objectTotalWeight = responseXml.ObjectTotalWeight,
                            unmatchedEssentialCount = responseXml.UnmatchedEssentialCount,
                            xacmlTrustFactor = (float) responseXml.XacmlTrustFactor,
                            final_trust_factor = 0,
                            final_result = false
                        });
                        return BadRequest("failed at XACML");
                    }

                    getNGACAccess = _mapper.Map<hybridFinalNGAC>(await evaluateHybridNGACAccess(userId, appId, permission_name));

                    if (getNGACAccess == null)
                    {
                        await _logFunction.LogModelDecisions(new Evaluation_results
                        {
                            scenario_id = scenarioId,
                            model_type = "Hybrid",
                            result_date = DateTime.Now,
                            xacml_result = responseXml.Result,
                            subjectWeightedScore = responseXml.SubjectWeightedScore,
                            subjectTotalWeight = responseXml.SubjectTotalWeight,
                            objectWeightedScore = responseXml.ObjectWeightedScore,
                            objectTotalWeight = responseXml.ObjectTotalWeight,
                            xacmlTrustFactor = (float) responseXml.XacmlTrustFactor,
                            unmatchedEssentialCount = responseXml.UnmatchedEssentialCount,
                            ngacTrustFactor = 0,
                            final_trust_factor = 0,
                            final_result = false
                        });
                        return BadRequest("failed at NGAC");
                    }
                    
                    getNGACAccess.NGACTrustFactor = (getNGACAccess.denyThreshold == 0 || getNGACAccess.denyCount == 0)
                    ? 1.0
                    : Math.Max(0, 1 - (double)getNGACAccess.denyCount / (getNGACAccess.denyThreshold + getNGACAccess.permitCount + 1));

                    totalTrust = responseXml.Result ? ((responseXml.XacmlTrustFactor * formulaConstants.xacmlConstant) + (getNGACAccess.NGACTrustFactor * formulaConstants.NGACConstant)) / (formulaConstants.xacmlConstant + formulaConstants.NGACConstant) : 0;
                    
                    await _logFunction.LogModelDecisions(new Evaluation_results
                        {
                            scenario_id = scenarioId,
                            model_type = "Hybrid",
                            result_date = DateTime.Now,
                            xacml_result = responseXml.Result,
                            subjectWeightedScore = responseXml.SubjectWeightedScore,
                            subjectTotalWeight = responseXml.SubjectTotalWeight,
                            objectWeightedScore = responseXml.ObjectWeightedScore,
                            objectTotalWeight = responseXml.ObjectTotalWeight,
                            xacmlTrustFactor = (float) responseXml.XacmlTrustFactor,
                            unmatchedEssentialCount = responseXml.UnmatchedEssentialCount,
                            ngacTrustFactor = (float) getNGACAccess.NGACTrustFactor,
                            denyCount = getNGACAccess.denyCount,
                            denyThreshold = getNGACAccess.denyThreshold,
                            permitCount = getNGACAccess.permitCount,
                            accessCount = getNGACAccess.accessCount,
                            final_trust_factor = (float) totalTrust,
                            final_result = totalTrust > 0.7 ? true : false
                        });
                }
            }


            return Ok(new {
                XACML_result = responseXml,
                NGAC_result = getNGACAccess,
                Final_trust_factor = totalTrust
                });
        }

        [HttpPost("testScenarios")]
        public async Task<ActionResult> testScenarios()
        {
            var getScenarios = await _context.scenarios.ToListAsync();
            foreach (var scenario in getScenarios)
            {
                await hybridAccess(scenario.scenario_id, scenario.user_id, scenario.resource_id, scenario.permission_name);
            }
            return Ok(getScenarios.Count + " done");
        }
    }
}