using System.Collections.Generic;

namespace DeskMind.Core.Security
{
    public class SecurityPolicy
    {
        private readonly List<ISecurityPolicyProvider> _providers = new();

        public void AddProvider(ISecurityPolicyProvider provider) => _providers.Add(provider);

        public bool Approve(string? userId, string[] requiredRoles)
        {
            foreach (var provider in _providers)
            {
                if (!provider.IsUserAllowed(userId, requiredRoles))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

