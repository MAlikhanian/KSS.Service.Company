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
            CreateMap<CompanyTranslation, CompanyTranslationDto>().ReverseMap();
            CreateMap<CompanyType, CompanyTypeDto>().ReverseMap();
            CreateMap<Industry, IndustryDto>().ReverseMap();
            CreateMap<StakeholderType, StakeholderTypeDto>().ReverseMap();
            CreateMap<CompanyStakeholder, CompanyStakeholderDto>().ReverseMap();
            CreateMap<CompanyStakeholderHistory, CompanyStakeholderHistoryDto>().ReverseMap();
            CreateMap<CompanyNameHistory, CompanyNameHistoryDto>().ReverseMap();

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
        }
    }
}