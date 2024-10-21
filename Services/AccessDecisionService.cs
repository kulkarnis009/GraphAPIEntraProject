using EntraGraphAPI.Data;

namespace EntraGraphAPI.Services
{
    // Services/AccessDecisionService.cs
    public class AccessDecisionService
    {
        public bool DecideAccess(string userRole, string documentType, string action)
        {
            foreach (var policy in PolicyStore.Policies)
            {
                if (policy.Subject == userRole && policy.Resource == documentType && policy.Action == action)
                {
                    return policy.Access;
                }
            }
            return false; // Default deny
        }
    }
}