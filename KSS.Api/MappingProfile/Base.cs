using AutoMapper;
using KSS.Entity;
using KSS.Dto;

namespace KSS.Api.MappingProfile
{
    public class BaseMappingProfile : Profile
    {
        public BaseMappingProfile()
        {
            // Company entities
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Translation, TranslationDto>().ReverseMap();
            CreateMap<CompanyInsertDto, Company>()
                .ForMember(dest => dest.Translations, opt => opt.Ignore()) // Translations handled separately
                .ForMember(dest => dest.NameHistories, opt => opt.Ignore()); // Name histories handled separately
            CreateMap<LegalForm, LegalFormDto>().ReverseMap();
            CreateMap<Industry, IndustryDto>().ReverseMap();
            CreateMap<StakeholderType, StakeholderTypeDto>().ReverseMap();
            CreateMap<Stakeholder, StakeholderDto>().ReverseMap();
            CreateMap<StakeholderHistory, StakeholderHistoryDto>().ReverseMap();
            CreateMap<NameHistory, NameHistoryDto>().ReverseMap();
            CreateMap<NameHistoryTranslation, NameHistoryTranslationDto>().ReverseMap();
            CreateMap<FinancialInfo, FinancialInfoDto>().ReverseMap();

            // Contact data
            CreateMap<EmailLabel, EmailLabelDto>().ReverseMap();
            CreateMap<EmailLabelTranslation, EmailLabelTranslationDto>().ReverseMap();
            CreateMap<Email, EmailDto>().ReverseMap();
            CreateMap<PhoneLabel, PhoneLabelDto>().ReverseMap();
            CreateMap<PhoneLabelTranslation, PhoneLabelTranslationDto>().ReverseMap();
            CreateMap<Phone, PhoneDto>().ReverseMap();
            CreateMap<AddressLabel, AddressLabelDto>().ReverseMap();
            CreateMap<AddressLabelTranslation, AddressLabelTranslationDto>().ReverseMap();
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<AddressTranslation, AddressTranslationDto>().ReverseMap();

            // Access + RoleAccess
            CreateMap<Access, AccessDto>().ReverseMap();
            CreateMap<AccessAddDto, Access>();
            CreateMap<RoleAccess, RoleAccessDto>().ReverseMap();
        }
    }
}
