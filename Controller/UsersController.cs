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


[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly GraphServiceClient _graphClient;

    // constructor to connect with service instance
    public UsersController(GraphApiService graphApiService)
    {
        _graphClient = graphApiService.GraphClient;
    }

    // Get endpoint to fetch user information
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _graphClient.Users.GetAsync();
        return Ok(users);
    }

    [HttpGet("specific/{UUID}")]
    public async Task<IActionResult> GetUserDetails(string UUID)
    {
        string[] columnString = new string[]
        {
        "DisplayName",
        "City",
        "CustomSecurityAttributes",
        "EmployeeHireDate",
        "EmployeeId",
        "JobTitle",
        "OfficeLocation",
        "EmployeeType",
        "Mail"
        };

        var user = await _graphClient.Users[UUID].GetAsync((requestConfiguration) =>
        {
            requestConfiguration.QueryParameters.Select = columnString;
        });
        // Using reflection to get property values by name
        var userInfo = new StringBuilder();
        Type userType = user.GetType();
        foreach (var columnName in columnString)
        {
            PropertyInfo propertyInfo = userType.GetProperty(columnName);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(user, null);
                userInfo.AppendLine($"{columnName}: {value}");
            }
        }
        return Ok(userInfo.ToString());
    }

    [HttpGet("specificcust/{UUID}")]
    public async Task<IActionResult> GetUserDetailsCust(string UUID)
    {
        var result = await _graphClient.Users[UUID].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { "customSecurityAttributes" };
            });

            if (result.CustomSecurityAttributes != null && result.CustomSecurityAttributes.AdditionalData != null)
            {
                foreach (var attribute in result.CustomSecurityAttributes.AdditionalData)
                {
                    if (attribute.Key == "devDetails")
                    {
                        System.Console.WriteLine("inside 1");

                        var json = JsonSerializer.Serialize(attribute.Value);
                        Console.WriteLine($"{attribute.Key}: {json}");
                        return Ok(json);
                    }
                }
            }
            else
            {
                Console.WriteLine("No custom security attributes found.");
            }
        return Ok();
    }

    [HttpPost("assignCust")]
    public async Task<ActionResult> assignCust()
    {
        var requestBody = new User
        {
            CustomSecurityAttributes = new CustomSecurityAttributeValue
            {
                AdditionalData = new Dictionary<string, object>
                {
                    {
                        "devDetails" , new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            {
                                "@odata.type", new UntypedString("#Microsoft.DirectoryServices.CustomSecurityAttributeValue")
                            },
                            {
                                "devLocation", new UntypedString("Argentina")
                            },
                        })
                    },
                },
            },
        };
        var result = await _graphClient.Users["{cf8f9e57-2d14-4043-9d15-39ffc3116a5f}"].PatchAsync(requestBody);
        return NoContent();
    }
}
