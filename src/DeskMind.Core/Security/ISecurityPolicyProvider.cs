namespace DeskMind.Core.Security
{
    public interface ISecurityPolicyProvider
    {
        bool IsUserAllowed(string? userId, string[] requiredRoles);
    }
}

