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
            CreateMap<CompanyOwnership, CompanyOwnershipViewDto>().ReverseMap();
            CreateMap<CompanyOwnershipInsertDto, CompanyOwnership>().ReverseMap();
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
            CreateMap<WebsiteLabel, WebsiteLabelDto>().ReverseMap();
            CreateMap<WebsiteLabelTranslation, WebsiteLabelTranslationDto>().ReverseMap();
            CreateMap<Website, WebsiteDto>().ReverseMap();
            CreateMap<SoftwareCategory, SoftwareCategoryDto>().ReverseMap();
            CreateMap<SoftwareCategoryTranslation, SoftwareCategoryTranslationDto>().ReverseMap();
            CreateMap<Software, SoftwareDto>().ReverseMap();
            CreateMap<CompanySoftware, CompanySoftwareDto>().ReverseMap();

            // Company documents + document-type lookup/translation
            CreateMap<CompanyDocument, CompanyDocumentViewDto>().ReverseMap();
            CreateMap<CompanyDocumentType, CompanyDocumentTypeDto>().ReverseMap();
            CreateMap<CompanyDocumentTypeTranslation, CompanyDocumentTypeTranslationDto>().ReverseMap();

            // Insert DTOs (no Id) — backend stamps the v7 GUID; a client GUID can't bind.
            CreateMap<AddressInsertDto, Address>();
            CreateMap<EmailInsertDto, Email>();
            CreateMap<PhoneInsertDto, Phone>();
            CreateMap<NameHistoryInsertDto, NameHistory>();
            CreateMap<FinancialInfoInsertDto, FinancialInfo>();
            CreateMap<StakeholderInsertDto, Stakeholder>();
            CreateMap<StakeholderHistoryInsertDto, StakeholderHistory>();
            CreateMap<CompanyDocumentInsertDto, CompanyDocument>();
            CreateMap<CompanyDocumentUpdateDto, CompanyDocument>();
            CreateMap<WebsiteInsertDto, Website>();
            CreateMap<SoftwareInsertDto, Software>();
            CreateMap<SoftwareUpdateDto, Software>();
            CreateMap<CompanySoftwareInsertDto, CompanySoftware>();

            // Access + RoleAccess
            CreateMap<Access, AccessDto>().ReverseMap();
            CreateMap<AccessAddDto, Access>();
            CreateMap<RoleAccess, RoleAccessDto>().ReverseMap();
        }
    }
}
