using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class XacmlPdpService
{
    private readonly HttpClient _httpClient;
    private readonly string _pdpEndpoint = "http://localhost:8080/authzforce-ce/domains/y3HEur_4Ee-_LwJCrBEAAg/pdp";

    public XacmlPdpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> EvaluatePolicyAsync(string role, string resource, string action)
{
    try
    {
        // Construct the XACML request XML
        var xacmlRequest = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Request xmlns=""urn:oasis:names:tc:xacml:3.0:core:schema:wd-17"" ReturnPolicyIdList=""true"" CombinedDecision=""false"">
  <Attributes Category=""urn:oasis:names:tc:xacml:1.0:subject-category:access-subject"">
    <Attribute AttributeId=""urn:oasis:names:tc:xacml:1.0:subject:subject-role"" IncludeInResult=""false"">
      <AttributeValue DataType=""http://www.w3.org/2001/XMLSchema#string"">{role}</AttributeValue>
    </Attribute>
  </Attributes>
  <Attributes Category=""urn:oasis:names:tc:xacml:3.0:resource-category:resource"">
    <Attribute AttributeId=""urn:oasis:names:tc:xacml:1.0:resource:id"" IncludeInResult=""false"">
      <AttributeValue DataType=""http://www.w3.org/2001/XMLSchema#string"">{resource}</AttributeValue>
    </Attribute>
  </Attributes>
  <Attributes Category=""urn:oasis:names:tc:xacml:3.0:action-category:action"">
    <Attribute AttributeId=""urn:oasis:names:tc:xacml:1.0:action:action-id"" IncludeInResult=""false"">
      <AttributeValue DataType=""http://www.w3.org/2001/XMLSchema#string"">{action}</AttributeValue>
    </Attribute>
  </Attributes>
</Request>
";

        // Log the request for debugging
        Console.WriteLine("XACML Request: " + xacmlRequest);

        // Create HTTP content
        var content = new StringContent(xacmlRequest, Encoding.UTF8, "application/xml");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml")
        {
            CharSet = "UTF-8"
        };

        // Set the Accept header
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

        // Send the request to the PDP
        var response = await _httpClient.PostAsync(_pdpEndpoint, content);

        // Ensure success
        response.EnsureSuccessStatusCode();

        // Read and return the response
        return await response.Content.ReadAsStringAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        throw;
    }
}



}
