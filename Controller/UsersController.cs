using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;

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

}
