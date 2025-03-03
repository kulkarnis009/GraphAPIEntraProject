using EntraGraphAPI.Data;
using EntraGraphAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Controllers
{
    public class TestResultsController : BaseApiController
    {
        private readonly DataContext _context;
        public TestResultsController(DataContext dataContext)
        {
            _context = dataContext;
        }

        [HttpGet("Scenarios")]
        public async Task<ActionResult<IEnumerable<Scenarios>>> GetScenarios()
        {
            var results = await _context.scenarios.ToListAsync();
            return Ok(results);
        }

        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<Evaluation_results>>> GetResult()
        {
            var results = await _context.evaluation_Results.ToListAsync();
            return Ok(results);
        }
    }
}