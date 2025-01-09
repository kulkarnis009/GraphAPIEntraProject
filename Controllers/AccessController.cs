using System.IdentityModel.Tokens.Jwt;
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

        [HttpPost("authorize")]
    public IActionResult Authorize([FromForm] string id_token)
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

        // Return User ID and App ID
        return Ok(new { userId, appId });
    }

    }

}