using Azure.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EntraGraphAPI.Service
{
    public class GraphApiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private static Azure.Core.AccessToken _cachedToken;
        private static readonly object _lock = new object();

        public GraphApiService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_configuration["AzureAd:GraphApiUrl"]);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var now = DateTime.UtcNow;
            lock (_lock)
            {
                if (_cachedToken.ExpiresOn > now.AddMinutes(5)) // Ensuring at least 5 minutes of validity
                {
                    System.Console.WriteLine(_cachedToken.ExpiresOn.ToString());
                    return _cachedToken.Token;
                }
            }
            var clientSecretCredential = new ClientSecretCredential(
                _configuration["AzureAd:TenantId"],
                _configuration["AzureAd:ClientId"],
                _configuration["AzureAd:ClientSecret"]
            );

            var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
            var accessToken = await clientSecretCredential.GetTokenAsync(tokenRequestContext);

            lock (_lock)
            {
                _cachedToken = accessToken;
            }
            return accessToken.Token;
        }

        public async Task<string> FetchGraphData(string endpoint)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
