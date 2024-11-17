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

            var oldRecords = _context.usersAttributes.Where(ua => ua.user_id == userId && ua.is_custom == false);
            _context.usersAttributes.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            var standardAttributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => sa.attribute_id);

            var userAttributesList = new List<UsersAttributes>();
            var standard_attribute = new StandardAttributes();
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

               if (!standardAttributes.TryGetValue(attribute.Key, out var attributeId))
                {
                    standard_attribute = new StandardAttributes
                    {
                        attribute_name = attribute.Key,
                        description = "user"
                    };
                    await _context.standard_attributes.AddAsync(standard_attribute);
                    await _context.SaveChangesAsync();
                    // Retrieve the newly generated attribute_id and update the dictionary
                    attributeId = standard_attribute.attribute_id;
                    standardAttributes[attribute.Key] = attributeId; // Add to dictionary for future use
                }
                userAttributesList.Add(new UsersAttributes
                {
                    user_id = userId,
                    attribute_id = attributeId,
                    attribute_value = attributeValue,
                    is_custom = false,
                    last_updated_date = DateTime.UtcNow
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
                        if (!standardAttributes.TryGetValue(riskAttribute, out var attributeId))
                        {
                            standard_attribute = new StandardAttributes
                            {
                                attribute_name = riskAttribute,
                                description = "user"
                            };
                            await _context.standard_attributes.AddAsync(standard_attribute);
                            await _context.SaveChangesAsync();
                            // Retrieve the newly generated attribute_id and update the dictionary
                            attributeId = standard_attribute.attribute_id;
                            standardAttributes[riskAttribute] = attributeId; // Add to dictionary for future use
                        }
                        userAttributesList.Add(new UsersAttributes
                        {
                            user_id = userId,
                            attribute_id = attributeId,
                            attribute_value = riskInfo[riskAttribute]?.ToString() ?? "null",
                            is_custom = false,
                            last_updated_date = DateTime.UtcNow
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
                                    // AttributeSet = setName,
                                    AttributeName = attribute.Name,
                                    attribute_value = attribute.Value.ToString(),
                                    last_updated_date = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    // Step 3: Fetch existing attributes for the user from the database
                    var existingAttributes = await _context.usersAttributes
                        .Where(ca => ca.user_id == userID && ca.is_custom == true)
                        .ToListAsync();
                    
                    _context.usersAttributes.RemoveRange(existingAttributes);
                    await _context.SaveChangesAsync();

                    var standardAttributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => sa.attribute_id);

                    var standard_attribute = new StandardAttributes();

                    // Step 4: Handle Updates and Additions
                    foreach (var customAttr in customAttributesList)
                    {
                         if (!standardAttributes.TryGetValue(customAttr.AttributeName, out var attributeId))
                        {
                            standard_attribute = new StandardAttributes
                            {
                                attribute_name = customAttr.AttributeName,
                                description = "user"
                            };
                            await _context.standard_attributes.AddAsync(standard_attribute);
                            await _context.SaveChangesAsync();
                            attributeId = standard_attribute.attribute_id;
                            standardAttributes[customAttr.AttributeName] = attributeId;
                        }

                        var addCustomAttributes = _mapper.Map<UsersAttributes>(customAttr);
                        addCustomAttributes.attribute_id = attributeId;
                        addCustomAttributes.is_custom = true;
                        // Add new attribute if it doesn't exist in the database
                        await _context.usersAttributes.AddAsync(addCustomAttributes);
                        
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

        [HttpGet("getLogs/{userId}/{hours}")]
        public async Task<ActionResult> getLogs(int userId, int hours)
        {
            var getUUID = await _context.users
                .Where(u => u.user_id == userId)
                .Select(u => u.id)
                .FirstOrDefaultAsync();

            if (getUUID == null) return BadRequest("Invalid user ID");

            // Calculate the start date-time based on the current time minus the specified number of hours
            DateTime startDate = DateTime.UtcNow.AddHours(-hours);

            // Format the endpoint with the calculated startDate
            var endpoint = $"auditLogs/signIns?$filter=userId eq '{getUUID}' " +
                            $"and createdDateTime ge {startDate:yyyy-MM-ddTHH:mm:ssZ}";

            var data = await _graphApiService.FetchGraphData(endpoint);

            // Deserialize the JSON data to LogAttributeDTO
            var logs = JsonConvert.DeserializeObject<GraphResponse>(data);

             var standardAttributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => sa.attribute_id);

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
            
            var standardAttributes = await _context.standard_attributes.ToDictionaryAsync(sa => sa.attribute_name, sa => sa.attribute_id);
            var standard_attribute = new StandardAttributes();
            foreach (var riskAttribute in riskAttributes)
            {
                if (!standardAttributes.TryGetValue(riskAttribute, out var attributeId))
                {
                    standard_attribute = new StandardAttributes
                    {
                        attribute_name = riskAttribute,
                        description = "user risk"
                    };
                    await _context.standard_attributes.AddAsync(standard_attribute);
                    await _context.SaveChangesAsync();
                    attributeId = standard_attribute.attribute_id;
                    standardAttributes[riskAttribute] = attributeId;
                }
            }

            var attributeIDs = standardAttributes.Where(sa => riskAttributes.Contains(sa.Key)).Select(sa => sa.Value).ToList();

            var oldRecords = await _context.usersAttributes.Where(ua => ua.user_id == userId && attributeIDs.Contains(ua.attribute_id)).ToListAsync();

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
                            attribute_id = standardAttributes[riskAttribute],
                            attribute_value = riskInfo[riskAttribute]?.ToString() ?? "null",
                            is_custom = false,
                            last_updated_date = DateTime.UtcNow
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

