using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


// Service to connect with Entra ID app using Microsoft Graph API
public class GraphApiService
{
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;
    public GraphServiceClient GraphClient => _graphClient;

    public GraphApiService(IConfiguration configuration)
    {
        _configuration = configuration;
        _graphClient = GetGraphServiceClient();
    }

    private GraphServiceClient GetGraphServiceClient()
    {
        var clientSecretCredential = new ClientSecretCredential(
            _configuration["AzureAd:TenantId"],
            _configuration["AzureAd:ClientId"],
            _configuration["AzureAd:ClientSecret"]
        );

        return new GraphServiceClient(clientSecretCredential, 
        new[] { "https://graph.microsoft.com/.default" });
    }
}
