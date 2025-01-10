using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AutoMapper;
using EntraGraphAPI.Data;
using EntraGraphAPI.Models;
using EntraGraphAPI.Service;
using EntraGraphAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Controllers
{
    public class AccessController : BaseApiController
    {
        private readonly AccessDecisionService _accessDecisionService = new AccessDecisionService();
        private readonly DataContext _context;
        private readonly UsersController _usersController;
        private readonly ApplicationController _applicationController;
        public AccessController(DataContext context, IMapper _mapper, GraphApiService _graphApiService)
        {
            _context = context;
            _usersController = new UsersController(_graphApiService,_context, _mapper);
            _applicationController = new ApplicationController(_graphApiService,_context,_mapper);
        }


        [HttpGet("{role}/{docType}/{actions}")]
        public async Task<ActionResult> GetDocumentAccess(string role, string docType, string actions)
        {
            bool isAllowed = _accessDecisionService.DecideAccess(role, docType, actions);
            return Ok(new { Role = role, DocumentType = docType, Action = actions, Access = isAllowed });
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

            System.Console.WriteLine(userId);
            System.Console.WriteLine(appId);
            await _usersController.GetSingleUserbyUUID(userId);
            System.Console.WriteLine("got attributes");
            
            var getAccess = await evaluateAccess(userId, appId);

            if (getAccess == null)
            {
                // Return an HTML page for "Access Denied"
                return Content("<html><body><h1>Access Denied</h1><p>You do not have the required permissions to access this resource.</p></body></html>", "text/html");
            }

            List<String>? getRedirect = await _applicationController.GetReplyUrlsByClientIdAsync(appId);
            // Return an HTML page to display the information
            string htmlResponse = $@"
            <html>
            <head>
                <script type='text/javascript'>
                    // Initialize the countdown value
                    var countdown = 10;

                    // Function to update the countdown text
                    function updateCountdown() {{
                        document.getElementById('countdown').innerText = countdown;
                        if (countdown === 0) {{
                            // Redirect to the target URL
                            window.location.href = '{getRedirect[0] ?? "#"}';
                        }} else {{
                            // Decrease the countdown and call the function again after 1 second
                            countdown--;
                            setTimeout(updateCountdown, 1000);
                        }}
                    }}

                    // Start the countdown when the page loads
                    window.onload = updateCountdown;
                </script>
            </head>
            <body>
                <h1>Access Granted</h1>
                <p><strong>ID:</strong> {getAccess.id}</p>
                <p><strong>Name:</strong> {getAccess.givenName + " " + getAccess.surname}</p>
                <p><strong>Resource ID:</strong> {getAccess.resource_id}</p>
                <p><strong>Permission Name:</strong> {getAccess.permission_name}</p>
                <p><strong>Description:</strong> {getAccess.description ?? "N/A"}</p>
                <p><strong>Redirect URL:</strong> <a href='{getRedirect[0] ?? "#"}'>{getRedirect[0] ?? "N/A"}</a></p>
                <p>You will be redirected in <span id='countdown'>10</span> seconds...</p>
            </body>
            </html>";
            return Content(htmlResponse, "text/html");

        }

        private async Task<evaluateNGACResult?> evaluateAccess(string userId, string appId)
        {
            var getAccessResult = await _context.evaluateAccessResults.FromSqlInterpolated($"Select * from evaluateAccess({userId}, {appId})").FirstOrDefaultAsync();
            
            if (getAccessResult == null) return null;
            
            return getAccessResult;
        }
    }

}