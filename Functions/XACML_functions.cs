using System.Xml.Linq;
namespace EntraGraphAPI.Functions
{
    public static class XACML_functions
    {

        public static string ParseDecision(string xmlResponse)
        {
            try
            {
                // Parse the XML response
                var doc = XDocument.Parse(xmlResponse);

                // Extract the Decision element
                var decision = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Decision")?.Value;

                return decision ?? "No Decision Found";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing decision: {ex.Message}");
                return "Error";
            }
        }

    }
}