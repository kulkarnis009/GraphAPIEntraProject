using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using EntraGreaphAPI.Service;

namespace EntraGreaphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GraphApiService _graphApiService;

        // constructor to connect with service instance
        public UsersController(GraphApiService graphApiService)
        {
            _graphApiService = graphApiService;
        }

            [HttpGet("specificcust/{UUID}")]
        public async Task<IActionResult> GetUserDetailsCust(string UUID)
        {
            var endpoint = $"users/{UUID}?$select=customSecurityAttributes";
            var data = await _graphApiService.FetchGraphData(endpoint);
            return Content(data, "application/json");
        }

        // [HttpPost("assignCust")]
        // public async Task<ActionResult> assignCust()
        // {
        //     var requestBody = new User
        //     {
        //         CustomSecurityAttributes = new CustomSecurityAttributeValue
        //         {
        //             AdditionalData = new Dictionary<string, object>
        //             {
        //                 {
        //                     "devDetails" , new UntypedObject(new Dictionary<string, UntypedNode>
        //                     {
        //                         {
        //                             "@odata.type", new UntypedString("#Microsoft.DirectoryServices.CustomSecurityAttributeValue")
        //                         },
        //                         {
        //                             "devLocation", new UntypedString("Argentina")
        //                         },
        //                     })
        //                 },
        //             },
        //         },
        //     };
        //     var result = await _graphClient.Users["{cf8f9e57-2d14-4043-9d15-39ffc3116a5f}"].PatchAsync(requestBody);
        //     return NoContent();
        // }
    }

}

