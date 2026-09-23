using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class WebsiteService : BaseService<Website, WebsiteDto, WebsiteInsertDto, WebsiteDto>, IWebsiteService
    {
        public WebsiteService(IMapper mapper, IWebsiteRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(Website item, bool saveChanges = true)
        {
            item.Url = WebsiteHelper.NormalizeUrl(item.Url);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(WebsiteInsertDto item, bool saveChanges = true)
        {
            item.Url = WebsiteHelper.NormalizeUrl(item.Url);
            var entity = _mapper.Map<Website>(item);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Website item, bool saveChanges = true)
        {
            item.Url = WebsiteHelper.NormalizeUrl(item.Url);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(WebsiteDto item, bool saveChanges = true)
        {
            item.Url = WebsiteHelper.NormalizeUrl(item.Url);
            var entity = _mapper.Map<Website>(item);
            base.Update(entity, saveChanges);
        }
    }
}
