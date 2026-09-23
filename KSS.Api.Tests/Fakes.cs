using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Api.Tests
{
    /// <summary>
    /// In-memory document repository. Every call is recorded, and any member the
    /// service is not expected to use throws, so an unexpected data access fails the
    /// test instead of passing silently.
    /// </summary>
    public class FakeDocumentRepository : DispatchProxy
    {
        public List<CompanyDocument> Rows { get; } = new();
        public List<string> Calls { get; } = new();
        public List<CompanyDocument> Added { get; } = new();
        public List<CompanyDocument> Updated { get; } = new();
        public List<CompanyDocument> Removed { get; } = new();

        public static (ICompanyDocumentRepository Repository, FakeDocumentRepository State) Create()
        {
            var proxy = Create<ICompanyDocumentRepository, FakeDocumentRepository>();
            return (proxy, (FakeDocumentRepository)(object)proxy);
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            var name = method!.Name;
            Calls.Add(name);

            switch (name)
            {
                case "Find" when args!.Length == 1:
                    var id = (Guid)args[0]!;
                    return Rows.FirstOrDefault(r => r.Id == id);

                case "ToListAsync" when args!.Length == 1 && args[0] is Expression<Func<CompanyDocument, bool>> filter:
                    return Task.FromResult<IEnumerable<CompanyDocument>>(Rows.Where(filter.Compile()).ToList());

                case "AddAsync" when args!.Length == 2:
                    Added.Add((CompanyDocument)args[0]!);
                    return Task.CompletedTask;

                case "Update" when args!.Length == 2:
                    Updated.Add((CompanyDocument)args[0]!);
                    return null;

                case "Remove" when args!.Length == 2:
                    Removed.Add((CompanyDocument)args[0]!);
                    return null;

                case "AddRangeAsync" when args!.Length == 2:
                    Added.AddRange((IEnumerable<CompanyDocument>)args[0]!);
                    return Task.CompletedTask;

                case "UpdateRange" when args!.Length == 2:
                    Updated.AddRange((IEnumerable<CompanyDocument>)args[0]!);
                    return null;

                case "RemoveRange" when args!.Length == 2:
                    Removed.AddRange((IEnumerable<CompanyDocument>)args[0]!);
                    return null;
            }

            throw new NotSupportedException("Unexpected repository call: " + name);
        }
    }

    /// <summary>Access service returning a configured Information level per company.</summary>
    public class FakeAccessService : DispatchProxy
    {
        public Dictionary<Guid, int> InformationLevels { get; } = new();
        public List<(Guid CompanyId, Guid CallerPersonId)> LevelQueries { get; } = new();

        public static (IAccessService Service, FakeAccessService State) Create()
        {
            var proxy = Create<IAccessService, FakeAccessService>();
            return (proxy, (FakeAccessService)(object)proxy);
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "GetLevelsAsync" && args!.Length == 2)
            {
                var companyId = (Guid)args[0]!;
                LevelQueries.Add((companyId, (Guid)args[1]!));
                var level = InformationLevels.TryGetValue(companyId, out var value) ? value : 0;
                return Task.FromResult(new AccessLevelsDto { Information = level });
            }

            throw new NotSupportedException("Unexpected access-service call: " + method.Name);
        }
    }

    /// <summary>
    /// Minimal mapper: carries the fields the service relies on from an insert DTO to
    /// the entity, and returns an empty destination for view mappings.
    /// </summary>
    public class FakeMapper : DispatchProxy
    {
        public static IMapper Create() => Create<IMapper, FakeMapper>();

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Map" && method.IsGenericMethod && args!.Length == 1)
            {
                var destination = method.ReturnType;

                if (destination == typeof(CompanyDocument) && args[0] is CompanyDocumentInsertDto dto)
                {
                    return new CompanyDocument
                    {
                        CompanyId = dto.CompanyId,
                        CompanyDocumentTypeId = dto.CompanyDocumentTypeId,
                        FileName = dto.FileName,
                    };
                }

                return Activator.CreateInstance(destination);
            }

            throw new NotSupportedException("Unexpected mapper call: " + method.Name);
        }
    }
}
