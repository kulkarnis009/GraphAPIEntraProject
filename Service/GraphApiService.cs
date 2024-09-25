using Azure.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GraphApiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GraphApiService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_configuration["AzureAd:GraphApiUrl"]);
    }

    public async Task<string> FetchGraphData(string endpoint)
    {
        var clientSecretCredential = new ClientSecretCredential(
            _configuration["AzureAd:TenantId"],
            _configuration["AzureAd:ClientId"],
            _configuration["AzureAd:ClientSecret"]
        );

        var accessToken = await clientSecretCredential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
