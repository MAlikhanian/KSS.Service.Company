using Microsoft.AspNetCore.Authorization;

namespace KSS.Helper.CustomAttribute
{
    /// <summary>
    /// Requires the authenticated user to have the specified permission claim in their JWT token.
    /// Usage: [HasPermission("Company.Create")]
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "Permission_";

        public HasPermissionAttribute(string permission)
            : base(PolicyPrefix + permission)
        {
        }
    }
}
