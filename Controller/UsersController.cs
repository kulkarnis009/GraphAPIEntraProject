using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using System.Collections.Generic;

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
        // var users = await _graphApiService.GetUsersAsync();
        var users = await _graphClient.Users.GetAsync();
        return Ok(users.Value);
    }

    [HttpGet("specific")]
    public async Task<IActionResult> GetSpecificUsers()
    {

        var user = await _graphClient.Users["1cb7d78f-13a5-4c61-ad96-f08c997a12ec"].GetAsync();
        Console.WriteLine(user.DisplayName);
        Console.WriteLine(user.JobTitle);
        Console.WriteLine(user.Mail);
        return Ok(user.DisplayName + " "+user.JobTitle+" "+user.Mail);
    }
}
