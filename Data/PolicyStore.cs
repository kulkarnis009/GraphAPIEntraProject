using System.Collections.Generic;
using EntraGraphAPI.Models;
namespace EntraGraphAPI.Data
{
    public static class PolicyStore
    {
        public static List<Policy> Policies = new List<Policy>
        {
            new Policy { Subject = "Employee", Resource = "Public", Action = "Read", Access = true },
            new Policy { Subject = "Manager", Resource = "Confidential", Action = "Read", Access = true },
            new Policy { Subject = "Manager", Resource = "Confidential", Action = "Write", Access = true }
        };
    }
}