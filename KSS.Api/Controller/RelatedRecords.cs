using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace KSS.Api.Controller
{
    /// <summary>
    /// Finds related records carried inside a write request. A request object is bound
    /// from the body with its navigation properties, and the data layer saves everything
    /// reachable from it, so a related record in the body would be written as well: into
    /// whatever company it names, or moved into the request's own. Write requests must
    /// carry the record itself and nothing reachable from it.
    /// </summary>
    public static class RelatedRecords
    {
        private static readonly Assembly EntityAssembly = typeof(KSS.Entity.Company).Assembly;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Navigations = new();

        /// <summary>Names of the navigation properties that carry a related record.</summary>
        public static IReadOnlyList<string> CarriedBy(object? item)
        {
            if (item == null) return Array.Empty<string>();
            var carried = new List<string>();
            foreach (var property in Navigations.GetOrAdd(item.GetType(), FindNavigations))
            {
                var value = property.GetValue(item);
                if (value is null) continue;
                if (value is IEnumerable sequence && value is not string)
                {
                    if (sequence.Cast<object?>().Any(x => x != null)) carried.Add(property.Name);
                }
                else
                {
                    carried.Add(property.Name);
                }
            }
            return carried;
        }

        // A navigation is a readable property whose type is an entity, or a sequence of entities.
        private static PropertyInfo[] FindNavigations(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsEntityOrEntitySequence(p.PropertyType))
                .ToArray();

        private static bool IsEntityOrEntitySequence(Type type)
        {
            if (IsEntity(type)) return true;
            if (type == typeof(string)) return false;
            var element = type.IsArray ? type.GetElementType()
                : type.GetInterfaces().Concat(type.IsInterface ? new[] { type } : Type.EmptyTypes)
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault();
            return element != null && IsEntity(element);
        }

        private static bool IsEntity(Type type) => type.IsClass && type.Assembly == EntityAssembly;
    }
}
