using System.Reflection;
using KSS.Api.Controller;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper.Authorization;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>
    /// Every action in the API must state a permission, either explicitly with
    /// [HasPermission] or through [PermissionGroup] plus an action name the permission
    /// filter maps to Read or Modify. An action that is reachable by any signed-in user
    /// must appear on the allow-list below, with its reason.
    /// </summary>
    public class AuthorizationSweepTests
    {
        private const string PendingScopedReads = "pending a decision on company-scoped reads";
        private const string CallerScoped = "caller-scoped: returns only the signed-in caller's own records, identified from the token";

        // Actions deliberately open to any signed-in user. Listing an entry here records
        // that it is known, not that it is approved.
        private static readonly Dictionary<string, string> OpenToAnySignedInUser = new(StringComparer.Ordinal)
        {
            ["AddressController.ByCompany"] = "read by a reporting service for a company's registration location; " + PendingScopedReads,
            ["FinancialInfoController.ByCompany"] = "read by a reporting service for registered capital; " + PendingScopedReads,
            ["CompanyController.Count"] = "dashboard total of companies; " + PendingScopedReads,
            ["AccessController.ListAllGrants"] = "read by a reporting service for (company, grantee) pairs; " + PendingScopedReads,
            ["AccessController.MyGrants"] = "the caller's own grants and the companies they name; " + CallerScoped,
        };

        private static readonly Assembly Api = typeof(CompanyController).Assembly;

        private static IReadOnlyDictionary<string, string> FilterMap()
        {
            var field = typeof(PermissionAuthorizationFilter)
                .GetField("ActionToOperation", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Permission filter map not found.");
            return (IReadOnlyDictionary<string, string>)field.GetValue(null)!;
        }

        private static IEnumerable<Type> Controllers() =>
            Api.GetTypes().Where(t =>
                t.IsClass && t.IsPublic && !t.IsAbstract && !t.ContainsGenericParameters &&
                typeof(ControllerBase).IsAssignableFrom(t));

        private static IEnumerable<MethodInfo> ActionsOf(Type controller) =>
            controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName)
                .Where(m => m.DeclaringType != typeof(object)
                         && m.DeclaringType != typeof(ControllerBase)
                         && m.DeclaringType != typeof(Microsoft.AspNetCore.Mvc.Controller))
                .Where(m => !m.GetCustomAttributes(inherit: true).OfType<NonActionAttribute>().Any());

        private static string TrimAsync(string name) =>
            name.EndsWith("Async", StringComparison.Ordinal) ? name[..^"Async".Length] : name;

        private static string Classify(Type controller, MethodInfo action, IReadOnlyDictionary<string, string> map)
        {
            var actionAttributes = action.GetCustomAttributes(inherit: true);
            var classAttributes = controller.GetCustomAttributes(inherit: true);

            if (actionAttributes.OfType<IAllowAnonymous>().Any() || classAttributes.OfType<IAllowAnonymous>().Any())
                return "ANONYMOUS";
            if (actionAttributes.OfType<HasPermissionAttribute>().Any() || classAttributes.OfType<HasPermissionAttribute>().Any())
                return "PERMISSION";
            if (classAttributes.OfType<PermissionGroupAttribute>().Any()
                && (map.ContainsKey(action.Name) || map.ContainsKey(TrimAsync(action.Name))))
                return "PERMISSION";
            if (actionAttributes.OfType<AuthorizeAttribute>().Any() || classAttributes.OfType<AuthorizeAttribute>().Any())
                return "SIGNED-IN ONLY";
            return "NO AUTHORIZATION";
        }

        [Fact]
        public void Every_action_states_a_permission_or_is_on_the_allow_list()
        {
            var map = FilterMap();
            var violations = new List<string>();

            foreach (var controller in Controllers())
            {
                foreach (var action in ActionsOf(controller))
                {
                    var kind = Classify(controller, action, map);
                    if (kind == "PERMISSION")
                        continue;

                    var key = controller.Name + "." + TrimAsync(action.Name);
                    if (kind == "SIGNED-IN ONLY" && OpenToAnySignedInUser.ContainsKey(key))
                        continue;

                    violations.Add(key + " -> " + kind);
                }
            }

            Assert.True(violations.Count == 0,
                "Actions without a permission and not on the allow-list:\n" + string.Join("\n", violations));
        }

        [Fact]
        public void Every_allow_list_entry_still_exists_and_is_still_open()
        {
            var map = FilterMap();
            var open = Controllers()
                .SelectMany(c => ActionsOf(c).Select(a => (Key: c.Name + "." + TrimAsync(a.Name), Kind: Classify(c, a, map))))
                .Where(x => x.Kind == "SIGNED-IN ONLY")
                .Select(x => x.Key)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var entry in OpenToAnySignedInUser)
                Assert.True(open.Contains(entry.Key), entry.Key + " is listed but is no longer open; remove it from the allow-list.");
        }

        [Fact]
        public void Every_allow_list_entry_carries_its_reason()
        {
            foreach (var entry in OpenToAnySignedInUser)
                Assert.True(entry.Value.Contains(PendingScopedReads, StringComparison.Ordinal) || entry.Value.Contains(CallerScoped, StringComparison.Ordinal),
                    entry.Key + " carries neither reason: " + entry.Value);
        }

        // ── Specific controllers ─────────────────────────────────────────────────

        [Fact]
        public void Ownership_is_not_exposed_as_an_endpoint()
        {
            Assert.DoesNotContain(Controllers(), t => t.Name == "CompanyOwnershipController");
        }

        // ── Document writes reached through BaseController ─────────────────────
        // The write actions BaseController exposes for every entity, by service method.
        // IBaseService also declares Add, AddRange, the *Unawaited variants and
        // Update(item, properties); no controller calls those, so they are not in this set.
        private static readonly string[] HttpReachableWrites =
        {
            "AddAsync", "AddDtoAsync", "AddRangeAsync",
            "Update", "UpdateDto", "UpdateRange",
            "Remove", "RemoveRange",
        };

        [Theory]
        [InlineData("AddAsync")]
        [InlineData("AddDtoAsync")]
        [InlineData("AddRangeAsync")]
        [InlineData("Update")]
        [InlineData("UpdateDto")]
        [InlineData("UpdateRange")]
        [InlineData("Remove")]
        [InlineData("RemoveRange")]
        public void Every_reachable_document_write_dispatches_to_the_document_service(string name)
        {
            // Resolved through the interface map: exactly how the controller's call through
            // IBaseService is dispatched. A method hidden with `new`, or one left inherited,
            // resolves to BaseService and fails here.
            var contract = typeof(IBaseService<CompanyDocument, CompanyDocumentViewDto, CompanyDocumentInsertDto, CompanyDocumentUpdateDto>);
            var map = typeof(CompanyDocumentService).GetInterfaceMap(contract);

            var targets = map.InterfaceMethods
                .Select((method, i) => (Contract: method, Target: map.TargetMethods[i]))
                .Where(x => x.Contract.Name == name)
                .Where(x => !(name == "Update" && x.Contract.GetParameters().Length != 2))
                .ToList();

            Assert.NotEmpty(targets);
            foreach (var target in targets)
                Assert.Equal(typeof(CompanyDocumentService), target.Target.DeclaringType);
        }

        [Fact]
        public void The_base_controller_exposes_no_write_outside_the_checked_set()
        {
            var writes = typeof(BaseController<,,,>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes(inherit: true).Any(a =>
                    a is HttpPostAttribute || a is HttpPutAttribute || a is HttpDeleteAttribute || a is HttpPatchAttribute))
                .Where(m => !m.Name.StartsWith("Find", StringComparison.Ordinal)
                         && !m.Name.StartsWith("Single", StringComparison.Ordinal)
                         && !m.Name.StartsWith("ToList", StringComparison.Ordinal))
                .Select(m => m.Name)
                .ToList();

            Assert.NotEmpty(writes);
            Assert.All(writes, write => Assert.Contains(write, HttpReachableWrites));
        }

        [Theory]
        [InlineData("ByCompany", "Company.Information.Read")]
        [InlineData("Create", "Company.Information.Modify")]
        [InlineData("ById", "Company.Information.Modify")]
        public void Document_actions_state_their_permission(string actionName, string permission)
        {
            var action = typeof(CompanyDocumentController).GetMethod(actionName)
                ?? throw new InvalidOperationException("No action named " + actionName);

            var attribute = Assert.Single(action.GetCustomAttributes(inherit: true).OfType<HasPermissionAttribute>());
            Assert.Equal(HasPermissionAttribute.PolicyPrefix + permission, attribute.Policy);
        }
    }
}
