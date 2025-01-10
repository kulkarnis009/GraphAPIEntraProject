using System.Text.Json;
using AutoMapper;
using EntraGraphAPI.Data;
using EntraGraphAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EntraGraphAPI.Controllers
{
    public class ApplicationController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly GraphApiService _graphApiService;
        private readonly IMapper _mapper;
        public ApplicationController(GraphApiService graphApiService, DataContext dataContext, IMapper mapper)
        {
            _context = dataContext;
            _graphApiService = graphApiService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> getApplications()
        {
            var endpoint = $"applications?$select=id,displayName,customAttributes";
            var data = await _graphApiService.FetchGraphData(endpoint);
            return Content(data, "application/json");
        }

        [HttpGet("specific/{clientId}")]
        public async Task<List<string>?> GetReplyUrlsByClientIdAsync(string clientId)
        {
            // Define the Graph API endpoint with filtering
            var endpoint = $"servicePrincipals?$filter=appId eq '{clientId}'";

            // Fetch the response
            var responseContent = await _graphApiService.FetchGraphData(endpoint);

            if (responseContent == null) return null;

            // Deserialize the response
            var servicePrincipals = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

            if (servicePrincipals != null && servicePrincipals.TryGetValue("value", out var value) && value is JArray servicePrincipalArray && servicePrincipalArray.Count > 0)
            {
                // Extract the "replyUrls" from the first matching service principal
                var firstServicePrincipal = servicePrincipalArray[0];
                if (firstServicePrincipal["replyUrls"] is JArray replyUrlsArray)
                {
                    // Filter out localhost URLs and return the list
                    var replyUrls = replyUrlsArray
                        .Select(url => url.ToString())
                        .Where(url => !url.StartsWith("https://localhost"))
                        .ToList();

                    return replyUrls;
                }
            }

            return null;
        }


    }
}