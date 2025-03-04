using AutoMapper;
using EntraGraphAPI.Data;
using EntraGraphAPI.Functions;
using EntraGraphAPI.Models;
using EntraGraphAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Controllers
{
    public class StandaloneController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly UsersController _usersController;
        private readonly ApplicationController _applicationController;
        private readonly XacmlPdpService _xacmlPdpService;
        private readonly Log_function _logFunction;
        private readonly IMapper _mapper;
        public StandaloneController(DataContext context, IMapper mapper, GraphApiService _graphApiService, XacmlPdpService xacmlPdpService)
        {
            _context = context;
            _usersController = new UsersController(_graphApiService, _context, mapper);
            _applicationController = new ApplicationController(_graphApiService, _context, mapper);
            _xacmlPdpService = xacmlPdpService;
            _logFunction = new Log_function(_context);
            _mapper = mapper;
        }

        //   Standalone NGAC and XACML
        private async Task<newEvaluateNGACResult?> evaluateNewNGACAccess(string userId, string appId)
        {
            System.Console.WriteLine("checking for " + userId + " " + appId);
            var getAccessResult = await _context.newEvaluateAccessResults.FromSqlInterpolated($"Select * from newEvaluateNGACaccess({userId}, {appId})").FirstOrDefaultAsync();

            if (getAccessResult == null) return null;
            return getAccessResult;
        }

        [HttpPost("newAuthorize/{scenarioId}/{userId}/{appId}/{permission_name}")]
        public async Task<ActionResult> standaloneModels(int scenarioId, string userId, string appId, string permission_name)
        {
            bool responseXml;
            // refreshing user attributes
            await _usersController.GetSingleUserbyUUID(userId);
            System.Console.WriteLine("got user attributes");

            await _usersController.GetUserDetailsCust(userId);
            System.Console.WriteLine("got user custom attributes");

            // await _usersController.getLogs(userId, appId, 750);
            // System.Console.WriteLine("got log attributes");

            var objectAttributes = await _context.getObjectAttributes.FromSqlInterpolated($"Select * from getObjectAttributes({appId}, {permission_name})").ToListAsync();

            if (objectAttributes != null)
            {

                var usersAttributes = await _context.getsubjectAttributes.FromSqlInterpolated($"Select * from getSubjectAttributes({userId})").ToListAsync();
                var getNGACAccess = await evaluateNewNGACAccess(userId, appId);

                if (getNGACAccess == null)
                {
                    await _logFunction.LogModelDecisions(new Evaluation_results
                    {
                        scenario_id = scenarioId,
                        model_type = "NGAC",
                        result_date = DateTime.Now,
                        final_result = false
                    });
                }
                else
                {
                    await _logFunction.LogModelDecisions(new Evaluation_results
                    {
                        scenario_id = scenarioId,
                        model_type = "NGAC",
                        result_date = DateTime.Now,
                        final_result = true
                    });
                }


                var standard_attributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => (sa.weight, sa.isEssential));

                responseXml = XACML_Replicate.ValidateXACMLSimple(objectAttributes, usersAttributes, permission_name);

                if (responseXml == false)
                {
                    await _logFunction.LogModelDecisions(new Evaluation_results
                    {
                        scenario_id = scenarioId,
                        model_type = "XACML",
                        result_date = DateTime.Now,
                        xacml_result = false,
                        final_result = false
                    });
                }
                else
                {
                    await _logFunction.LogModelDecisions(new Evaluation_results
                    {
                        scenario_id = scenarioId,
                        model_type = "XACML",
                        result_date = DateTime.Now,
                        xacml_result = true,
                        final_result = true
                    });
                }
            }
            return Ok("done");
        }

        [HttpPost("testScenarios")]
        public async Task<ActionResult> testScenarios()
        {
            var getScenarios = await _context.scenarios.ToListAsync();
            foreach (var scenario in getScenarios)
            {
                await standaloneModels(scenario.scenario_id, scenario.user_id, scenario.resource_id, scenario.permission_name);
            }
            
        return Ok(getScenarios.Count + " done");
        }
    }
}