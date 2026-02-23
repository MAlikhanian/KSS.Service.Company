using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using KSS.Helper.CustomAttribute;

namespace KSS.Helper.Authorization
{
    /// <summary>
    /// Authorization filter that automatically enforces CRUD permissions
    /// based on the controller's [PermissionGroup] and the action being invoked.
    ///
    /// Action name mapping:
    ///   Read   → FindAsync, SingleAsync, ToListAllAsync, ToListAsync, ToListByFilterAsync, ToListDtoAsync
    ///   (Custom actions like FindByCompanyIdAsync, FindByPersonIdAsync skip CRUD permission — only require [Authorize])
    ///   Create → AddAsync, AddDtoAsync, AddRangeAsync
    ///   Update → Update, UpdateDto, UpdateRange
    ///   Delete → Remove, RemoveRange
    /// </summary>
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;

        // Map action names to CRUD operations
        private static readonly Dictionary<string, string> ActionToOperation = new(StringComparer.OrdinalIgnoreCase)
        {
            // Read operations
            { "FindAsync",            "Read" },
            { "SingleAsync",          "Read" },
            { "ToListAllAsync",       "Read" },
            { "ToListAsync",          "Read" },
            { "ToListByFilterAsync",  "Read" },
            { "ToListDtoAsync",       "Read" },

            // Create operations
            { "AddAsync",             "Create" },
            { "AddDtoAsync",          "Create" },
            { "AddRangeAsync",        "Create" },

            // Update operations
            { "Update",               "Update" },
            { "UpdateDto",            "Update" },
            { "UpdateRange",          "Update" },

            // Delete operations
            { "Remove",               "Delete" },
            { "RemoveRange",          "Delete" },
        };

        public PermissionAuthorizationFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the controller descriptor
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return;

            // Check if controller has [PermissionGroup] attribute
            var permissionGroup = controllerActionDescriptor.ControllerTypeInfo
                .GetCustomAttribute<PermissionGroupAttribute>();
            if (permissionGroup == null) return; // No group = skip this filter

            // Map action name to operation
            var actionName = controllerActionDescriptor.ActionName;
            if (!ActionToOperation.TryGetValue(actionName, out var operation))
                return; // Unknown action = skip (let other auth handle it)

            // Build the required permission: e.g., "Email.Create"
            var requiredPermission = $"{permissionGroup.Group}.{operation}";

            // Check authorization
            var requirement = new PermissionRequirement(requiredPermission);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(requirement)
                .Build();

            var result = await _authorizationService.AuthorizeAsync(context.HttpContext.User, policy);

            if (!result.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
