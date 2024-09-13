using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

    // Get endpoint to fetch user information
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _graphApiService.GetUsersAsync();
        return Ok(users);
    }
}
