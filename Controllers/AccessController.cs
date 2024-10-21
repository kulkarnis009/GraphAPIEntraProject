using EntraGraphAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EntraGraphAPI.Controllers
{
    public class AccessController : BaseApiController
    {
        private readonly AccessDecisionService _accessDecisionService = new AccessDecisionService();

        [HttpGet("{role}/{docType}/{actions}")]
        public async Task<ActionResult> GetDocumentAccess(string role, string docType, string actions)
        {
            bool isAllowed = _accessDecisionService.DecideAccess(role, docType, actions);
            return Ok(new { Role = role, DocumentType = docType, Action = actions, Access = isAllowed });
        }
    }

}