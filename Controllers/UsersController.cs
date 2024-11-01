using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using EntraGraphAPI.Wrapper;
using EntraGraphAPI.Models;
using EntraGraphAPI.Dto;
using EntraGraphAPI.Data;
using EntraGraphAPI.Service;
using Newtonsoft.Json.Linq;

namespace EntraGraphAPI.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly GraphApiService _graphApiService;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        // constructor to connect with service instance
        public UsersController(GraphApiService graphApiService, DataContext dataContext, IMapper mapper)
        {
            _graphApiService = graphApiService;
            _context = dataContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var endpoint = $"users";
            var data = await _graphApiService.FetchGraphData(endpoint);

            List<RecieveUsers> recieveUsers = new List<RecieveUsers>();
            DateTime getDate = DateTime.UtcNow;

            using (JsonDocument doc = JsonDocument.Parse(data))
            {
                if (doc.RootElement.TryGetProperty("value", out JsonElement recieveUsersList))
                {

                    foreach (JsonElement userElement in recieveUsersList.EnumerateArray())
                    {
                        recieveUsers.Add(new RecieveUsers
                        {
                            id = userElement.GetProperty("id").GetString(),
                            givenName = userElement.GetProperty("givenName").GetString(),
                            surname = userElement.TryGetProperty("surname", out JsonElement surnameElement) ? surnameElement.GetString() : null,

                        });
                    }

                    // Step 2: Fetch existing usersfrom the database
                    var recievedUserIds = recieveUsers.Select(r => r.id).ToList();
                    var existingUsers = await _context.users
                        .Where(ca => recievedUserIds.Contains(ca.id))
                        .ToListAsync();

                    // Step 3: Handle Updates and Additions
                    foreach (var recievedUser in recieveUsers)
                    {
                        var existingUser = existingUsers.FirstOrDefault(eu => eu.id == recievedUser.id);

                        if (existingUser != null)
                        {
                            // Update the existing user's fields
                            existingUser.givenName = recievedUser.givenName;
                            existingUser.surname = recievedUser.surname;
                        }
                        else
                        {
                            // Add new user if it doesn't exist
                            var addUser = _mapper.Map<Users>(recievedUser);
                            await _context.users.AddAsync(addUser);
                        }
                    }

                    // Step 4: Handle Deletions
                    // Find users that exist in the database but are not present in the incoming data
                    // Step 4: Handle Deletions
                    var usersToDelete = existingUsers
                        .Where(eu => !recieveUsers.Any(ru => ru.id == eu.id))
                        .ToList();

                    if (usersToDelete.Any())
                    {
                        _context.users.RemoveRange(usersToDelete);
                    }
                    await _context.SaveChangesAsync();

                    return Ok("User data added/updated successfully.");
                }
                else
                {
                    return BadRequest("No user data available.");
                }
            }
        }

        [HttpGet("UUID/{UUID}")]
        public async Task<IActionResult> GetSingleUserbyUUID(string UUID)
        {
            var selectedAttributes = "id,displayName,givenName,surname,jobTitle,department,mail,officeLocation,userPrincipalName,signInActivity";

            // Modify the endpoint URL to include only the selected attributes
            var endpoint = $"users/{UUID}?$select={selectedAttributes}";
            var data = await _graphApiService.FetchGraphData(endpoint);

            // Parse JSON data (assuming 'data' is a JSON string)
            var userAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            // Set user_id here if you have a way to retrieve it; alternatively, link this to the UUID
            int userId = await _context.users.Where(u => u.id == UUID).Select(u => u.user_id).FirstOrDefaultAsync();

            var oldRecords = _context.usersAttributes.Where(ua => ua.user_id == userId);
            _context.usersAttributes.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            var userAttributesList = new List<UsersAttributes>();
            foreach (var attribute in userAttributes)
            {
                // Skip "@odata.context"
                if (attribute.Key == "@odata.context" || attribute.Key == "id") continue;

                // Handle businessPhones as comma-separated string
                // Handle signInActivity separately if you need only the last login time
                string attributeValue;
                if (attribute.Key == "signInActivity" && attribute.Value is JObject activity)
                {
                    attributeValue = activity["lastSignInDateTime"]?.ToString() ?? "null";
                }
                else if (attribute.Key == "businessPhones" && attribute.Value is JArray phonesArray)
                {
                    attributeValue = string.Join(",", phonesArray.ToObject<List<string>>());
                }
                else
                {
                    attributeValue = attribute.Value?.ToString() ?? "null";
                }

                userAttributesList.Add(new UsersAttributes
                {
                    user_id = userId,
                    AttributeName = attribute.Key,
                    AttributeValue = attributeValue,
                    LastUpdatedDate = DateTime.UtcNow
                });
            }

            // Fetch risk score from a separate endpoint if available
            var riskEndpoint = $"identityProtection/riskyUsers/{UUID}";
            var riskData = String.Empty;
            try
            {
                riskData = await _graphApiService.FetchGraphData(riskEndpoint);
                var riskInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(riskData);

                string[] riskAttributes = { "riskLevel", "riskState", "riskDetail", "riskLastUpdatedDateTime" };

                foreach (var riskAttribute in riskAttributes)
                {
                    if (riskInfo.ContainsKey(riskAttribute))
                    {
                        userAttributesList.Add(new UsersAttributes
                        {
                            user_id = userId,
                            AttributeName = riskAttribute,
                            AttributeValue = riskInfo[riskAttribute]?.ToString() ?? "null",
                            LastUpdatedDate = DateTime.UtcNow
                        });
                    }
                }

            }
            catch (Exception ex)
            {
            }

            // Save to the database
            await _context.usersAttributes.AddRangeAsync(userAttributesList);
            await _context.SaveChangesAsync();

            return Content(riskData, "application/json");
        }

        [HttpGet("userID/{userId}")]
        public async Task<IActionResult> GetSingleUser(int userId)
        {
            var getUUID = await _context.users.Where(u => u.user_id == userId).Select(u => u.id).FirstOrDefaultAsync();

            if (getUUID == null) return BadRequest("invalid user id");

            var data = await GetSingleUserbyUUID(getUUID);
            return data;
        }

        [HttpGet("customattr/{userID}")]
        public async Task<ActionResult> GetUserDetailsCust(int userID)
        {
            // step 1: getting customer UUID
            var getUUID = await _context.users.Where(u => u.user_id == userID).Select(u => u.id).FirstOrDefaultAsync();

            // step 2: getting custom attributes for the user from Entra ID
            var endpoint = $"users/{getUUID}?$select=customSecurityAttributes";
            var data = await _graphApiService.FetchGraphData(endpoint);
            List<ReceiveCustomAttributes> customAttributesList = new List<ReceiveCustomAttributes>();
            DateTime getDate = DateTime.UtcNow;

            using (JsonDocument doc = JsonDocument.Parse(data))
            {
                if (doc.RootElement.TryGetProperty("customSecurityAttributes", out JsonElement customAttributes))
                {
                    if (customAttributes.ValueKind != JsonValueKind.Object) return NoContent();
                    foreach (JsonProperty attributeSet in customAttributes.EnumerateObject())
                    {
                        string setName = attributeSet.Name;

                        foreach (JsonProperty attribute in attributeSet.Value.EnumerateObject())
                        {
                            if (!attribute.Name.Equals("@odata.type"))
                            {
                                customAttributesList.Add(new ReceiveCustomAttributes
                                {
                                    user_id = userID,
                                    id = getUUID,
                                    AttributeSet = setName,
                                    AttributeName = attribute.Name,
                                    AttributeValue = attribute.Value.ToString(),
                                    LastUpdatedDate = getDate
                                });
                            }
                        }
                    }

                    // Step 3: Fetch existing attributes for the user from the database
                    var existingAttributes = await _context.customAttributes
                        .Where(ca => ca.user_id.Equals(userID))
                        .ToListAsync();

                    // Step 4: Handle Updates and Additions
                    foreach (var customAttr in customAttributesList)
                    {
                        // Check if the attribute already exists in the database
                        var existingAttr = existingAttributes.FirstOrDefault(ea =>
                            ea.AttributeSet == customAttr.AttributeSet &&
                            ea.AttributeName == customAttr.AttributeName);

                        if (existingAttr != null)
                        {
                            // Update the existing attribute's value and last updated date
                            existingAttr.AttributeValue = customAttr.AttributeValue;
                            existingAttr.LastUpdatedDate = getDate;
                        }
                        else
                        {
                            var addCustomAttributes = _mapper.Map<CustomAttributes>(customAttr);
                            // Add new attribute if it doesn't exist in the database
                            await _context.customAttributes.AddAsync(addCustomAttributes);
                        }
                    }

                    // Step 5: Handle Deletions
                    // Find attributes that exist in the database but are not present in the incoming data
                    var attributesToDelete = existingAttributes
                        .Where(ea => !customAttributesList.Any(ca =>
                            ca.AttributeSet == ea.AttributeSet &&
                            ca.AttributeName == ea.AttributeName))
                        .ToList();

                    // Remove attributes that are no longer present in the incoming data
                    if (attributesToDelete.Any())
                    {
                        _context.customAttributes.RemoveRange(attributesToDelete);
                    }
                    await _context.SaveChangesAsync();

                    return Ok("Custom attributes added/updated successfully.");
                }
                else
                {
                    return BadRequest("No custom security attributes available.");
                }
            }
        }

        [HttpGet("getLogs/{userId}/{startDate}/{endDate}")]
        public async Task<ActionResult> getLogs(int userId, DateTime startDate, DateTime endDate)
        {
            var getUUID = await _context.users
                .Where(u => u.user_id == userId)
                .Select(u => u.id)
                .FirstOrDefaultAsync();

            if (getUUID == null) return BadRequest("Invalid user ID");

            var endpoint = $"auditLogs/signIns?$filter=userId eq '{getUUID}' " +
                        $"and createdDateTime ge {startDate:yyyy-MM-ddTHH:mm:ssZ} ";

            var data = await _graphApiService.FetchGraphData(endpoint);

            // Deserialize the JSON data to LogAttributeDTO
            var logs = JsonConvert.DeserializeObject<GraphResponse>(data);

            // Map each LogAttributeDTO to LogAttribute and store in the database
            foreach (var logDto in logs.value)
            {
                var logEntry = _mapper.Map<LogAttribute>(logDto);
                logEntry.UserId = userId;  // Set the user_id foreign key

                await _context.logAttributes.AddAsync(logEntry);
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("Log data saved to database.");
            return Ok("Log data stored successfully.");
        }

        [HttpGet("getUserRisk/{userId}")]
        public async Task<ActionResult> getUserRisk(int userId)
        {
            var getUUID = await _context.users
                .Where(u => u.user_id == userId)
                .Select(u => u.id)
                .FirstOrDefaultAsync();

            var endpoint = $"identityProtection/riskyUsers/{getUUID}";

            string[] riskAttributes = { "riskLevel", "riskState", "riskDetail", "riskLastUpdatedDateTime" };
            
            var oldRecords = _context.usersAttributes.Where(ua => ua.user_id == userId && riskAttributes.Contains(ua.AttributeName));
            _context.usersAttributes.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            var riskData = string.Empty;
            var userAttributesList = new List<UsersAttributes>();
            try
            {
                riskData = await _graphApiService.FetchGraphData(endpoint);
                var riskInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(riskData);


                foreach (var riskAttribute in riskAttributes)
                {
                    if (riskInfo.ContainsKey(riskAttribute))
                    {
                        userAttributesList.Add(new UsersAttributes
                        {
                            user_id = userId,
                            AttributeName = riskAttribute,
                            AttributeValue = riskInfo[riskAttribute]?.ToString() ?? "null",
                            LastUpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                // Save to the database
                await _context.usersAttributes.AddRangeAsync(userAttributesList);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }

            return Content(riskData, "application/json");
        }

        // [HttpPost("assignCust")]
        // public async Task<ActionResult> assignCust()
        // {
        //     var requestBody = new User
        //     {
        //         CustomSecurityAttributes = new CustomSecurityAttributeValue
        //         {
        //             AdditionalData = new Dictionary<string, object>
        //             {
        //                 {
        //                     "devDetails" , new UntypedObject(new Dictionary<string, UntypedNode>
        //                     {
        //                         {
        //                             "@odata.type", new UntypedString("#Microsoft.DirectoryServices.CustomSecurityAttributeValue")
        //                         },
        //                         {
        //                             "devLocation", new UntypedString("Argentina")
        //                         },
        //                     })
        //                 },
        //             },
        //         },
        //     };
        //     var result = await _graphClient.Users["{cf8f9e57-2d14-4043-9d15-39ffc3116a5f}"].PatchAsync(requestBody);
        //     return NoContent();
        // }
    }

}

