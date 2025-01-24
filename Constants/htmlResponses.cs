using EntraGraphAPI.Functions;
using EntraGraphAPI.Models;

namespace EntraGraphAPI.Constants
{
    public static class htmlResponses
    {
        public static String denyResponse = "<html><body><h1>Access Denied by NGAC</h1><p>You do not have the required permissions to access this resource.</p></body></html>";
        public static String denyResponseXACML = "<html><body><h1>Access Denied by XACML</h1><p>You do not have the required permissions to access this resource.</p></body></html>";
        public static String getSuccessResponse(evaluateNGACResult getAccess, String XACMLResult, List<String>? getRedirect)
        {
            String htmlResponse = $@"
            <html>
            <head>
                <script type='text/javascript'>
                    // Initialize the countdown value
                    var countdown = 10;

                    // Function to update the countdown text
                    function updateCountdown() {{
                        document.getElementById('countdown').innerText = countdown;
                        if (countdown === 0) {{
                            // Redirect to the target URL
                            window.location.href = '{getRedirect[0] ?? "#"}';
                        }} else {{
                            // Decrease the countdown and call the function again after 1 second
                            countdown--;
                            setTimeout(updateCountdown, 1000);
                        }}
                    }}

                    // Start the countdown when the page loads
                    window.onload = updateCountdown;
                </script>
            </head>
            <body>
                <h1>Access Granted</h1>
                <p><strong>ID:</strong> {getAccess.id}</p>
                <p><strong>Name:</strong> {getAccess.givenName + " " + getAccess.surname}</p>
                <p><strong>Resource ID:</strong> {getAccess.resource_id}</p>
                <p><strong>Permission Name:</strong> {getAccess.permission_name}</p>
                <p><strong>Description:</strong> {getAccess.description ?? "N/A"}</p>
                <p><strong>XACML result:</strong> {XACMLResult?? "N/A"}</p>
                <p><strong>Redirect URL:</strong> <a href='{getRedirect[0] ?? "#"}'>{getRedirect[0] ?? "N/A"}</a></p>
                <p>You will be redirected in <span id='countdown'>10</span> seconds...</p>
            </body>
            </html>";

            return htmlResponse;
        }
    }
}