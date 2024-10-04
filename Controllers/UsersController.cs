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
using EntraGreaphAPI.Models;
using EntraGreaphAPI.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace EntraGreaphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GraphApiService _graphApiService;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        // constructor to connect with service instance
        public UsersController(GraphApiService graphApiService, DataContext dataContext, IMapper mapper)
        {
            _graphApiService = graphApiService;
            _context = dataContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var endpoint = $"users";
            var data = await _graphApiService.FetchGraphData(endpoint);
            return Content(data, "application/json");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSingleUser(string userId)
        {
            var endpoint = $"users/{userId}";
            var data = await _graphApiService.FetchGraphData(endpoint);
            return Content(data, "application/json");
        }

        [HttpGet("customattr/{UUID}")]
        public async Task<ActionResult> GetUserDetailsCust(string UUID)
        {
            // step 1: getting custom attributes for the user from Entra ID
            var endpoint = $"users/{UUID}?$select=customSecurityAttributes";
            var data = await _graphApiService.FetchGraphData(endpoint);

            List<ReceiveCustomAttributes> customAttributesList = new List<ReceiveCustomAttributes>();
            DateTime getDate = DateTime.UtcNow;

            using (JsonDocument doc = JsonDocument.Parse(data))
            {
                if (doc.RootElement.TryGetProperty("customSecurityAttributes", out JsonElement customAttributes))
                {
                    foreach (JsonProperty attributeSet in customAttributes.EnumerateObject())
                    {
                        string setName = attributeSet.Name;
                        
                        foreach (JsonProperty attribute in attributeSet.Value.EnumerateObject())
                        {
                            if(!attribute.Name.Equals("@odata.type"))
                            {
                                customAttributesList.Add(new ReceiveCustomAttributes
                                {
                                    user_id = UUID,
                                    AttributeSet = setName,
                                    AttributeName = attribute.Name,
                                    AttributeValue = attribute.Value.ToString(),
                                    LastUpdatedDate = getDate
                                });
                            }
                        }
                    }
                    
                    // Step 2: Fetch existing attributes for the user from the database
                    var existingAttributes = await _context.customAttributes
                        .Where(ca => ca.user_id.Equals(UUID))
                        .ToListAsync();

                    // Step 3: Handle Updates and Additions
                    foreach (var customAttr in customAttributesList)
                    {
                        // Check if the attribute already exists in the database
                        var existingAttr = existingAttributes.FirstOrDefault(ea =>
                            ea.AttributeSet == customAttr.AttributeSet &&
                            ea.AttributeName == customAttr.AttributeName);

                        if (existingAttr != null)
                        {
                            // Update the existing attribute's value and last updated date
                            existingAttr.AttributeValue = customAttr.AttributeValue;
                            existingAttr.LastUpdatedDate = getDate;
                        }
                        else
                        {
                            var addCustomAttributes = _mapper.Map<CustomAttributes>(customAttr);
                            // Add new attribute if it doesn't exist in the database
                            await _context.customAttributes.AddAsync(addCustomAttributes);
                        }
                    }

                    // Step 4: Handle Deletions
                    // Find attributes that exist in the database but are not present in the incoming data
                    var attributesToDelete = existingAttributes
                        .Where(ea => !customAttributesList.Any(ca =>
                            ca.AttributeSet == ea.AttributeSet &&
                            ca.AttributeName == ea.AttributeName))
                        .ToList();

                    // Remove attributes that are no longer present in the incoming data
                    if (attributesToDelete.Any())
                    {
                        _context.customAttributes.RemoveRange(attributesToDelete);
                    }
                    await _context.SaveChangesAsync();

                    return Ok("Custom attributes added/updated successfully.");
                }
                else
                {
                    return BadRequest("No custom security attributes available.");
                }
            }
        }

        [HttpGet("getLogs/{userUUID}/{startDate}/{endDate}/{deviceType}")]
        public async Task<ActionResult> getLogs(string userUUID, DateTime startDate, DateTime endDate, string deviceType)
        {
            var endpoint = $"auditLogs/signIns?$filter=userId eq '{userUUID}' " +
                   $"and createdDateTime ge {startDate:yyyy-MM-ddTHH:mm:ssZ} " +
                   $"and clientAppUsed eq '{deviceType}'";
            
            var data = await _graphApiService.FetchGraphData(endpoint);
            System.Console.WriteLine("got the logs");
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

