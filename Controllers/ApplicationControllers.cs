using AutoMapper;
using EntraGraphAPI.Data;
using EntraGraphAPI.Service;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("specific/{id}")]
        public async Task<ActionResult> getSpecificApplication(string id)
        {
            var endpoint = $"applications/{id}?$select=id,displayName,customSecurityAttributes";
            var data = await _graphApiService.FetchGraphData(endpoint);
            return Content(data, "application/json");
        }
    }
}