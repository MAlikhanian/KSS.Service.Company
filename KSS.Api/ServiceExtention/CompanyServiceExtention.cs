using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;

namespace KSS.Api.ServiceExtention
{
    public static class CompanyServiceExtention
    {
        public static IServiceCollection AddCompanyServiceExtention(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            services.AddDbContext<MainDbContext>(options => options.UseSqlServer(connectionString));

            // Lookup services
            services.AddScoped<ILegalFormRepository, LegalFormRepository>();
            services.AddScoped<ILegalFormService, LegalFormService>();
            services.AddScoped<IIndustryRepository, IndustryRepository>();
            services.AddScoped<IIndustryService, IndustryService>();
            services.AddScoped<IStakeholderTypeRepository, StakeholderTypeRepository>();
            services.AddScoped<IStakeholderTypeService, StakeholderTypeService>();
            services.AddScoped<IEmailLabelRepository, EmailLabelRepository>();
            services.AddScoped<IEmailLabelService, EmailLabelService>();
            services.AddScoped<IEmailLabelTranslationRepository, EmailLabelTranslationRepository>();
            services.AddScoped<IEmailLabelTranslationService, EmailLabelTranslationService>();
            services.AddScoped<IPhoneLabelRepository, PhoneLabelRepository>();
            services.AddScoped<IPhoneLabelService, PhoneLabelService>();
            services.AddScoped<IPhoneLabelTranslationRepository, PhoneLabelTranslationRepository>();
            services.AddScoped<IPhoneLabelTranslationService, PhoneLabelTranslationService>();
            services.AddScoped<IAddressLabelRepository, AddressLabelRepository>();
            services.AddScoped<IAddressLabelService, AddressLabelService>();
            services.AddScoped<IAddressLabelTranslationRepository, AddressLabelTranslationRepository>();
            services.AddScoped<IAddressLabelTranslationService, AddressLabelTranslationService>();

            // Main Company services
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ITranslationRepository, TranslationRepository>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<INameHistoryRepository, NameHistoryRepository>();
            services.AddScoped<INameHistoryService, NameHistoryService>();
            services.AddScoped<INameHistoryTranslationRepository, NameHistoryTranslationRepository>();
            services.AddScoped<INameHistoryTranslationService, NameHistoryTranslationService>();
            services.AddScoped<ICompanyNameManagementService, CompanyNameManagementService>();
            services.AddScoped<ICompanyOperationService, CompanyOperationService>();
            services.AddScoped<ICompanySelectService, CompanySelectService>();
            services.AddScoped<ICompanyDetailService, CompanyDetailService>();
            services.AddScoped<ICompanyReadViewManagementService, CompanyReadViewManagementService>();
            services.AddScoped<IStakeholderRepository, StakeholderRepository>();
            services.AddScoped<IStakeholderService, StakeholderService>();
            services.AddScoped<IStakeholderHistoryRepository, StakeholderHistoryRepository>();
            services.AddScoped<IStakeholderHistoryService, StakeholderHistoryService>();
            services.AddScoped<ICompanyStakeholderManagementService, CompanyStakeholderManagementService>();
            services.AddScoped<IFinancialInfoRepository, FinancialInfoRepository>();
            services.AddScoped<IFinancialInfoService, FinancialInfoService>();

            // Contact data services
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPhoneRepository, PhoneRepository>();
            services.AddScoped<IPhoneService, PhoneService>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAddressTranslationRepository, AddressTranslationRepository>();
            services.AddScoped<IAddressTranslationService, AddressTranslationService>();
            services.AddScoped<ICompanyContactService, CompanyContactService>();

            // Access + RoleAccess (per-section access on companies)
            services.AddScoped<IAccessRepository, AccessRepository>();
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<IRoleAccessRepository, RoleAccessRepository>();
            services.AddScoped<IRoleAccessService, RoleAccessService>();

            // Required so AccessService can read JWT claims for the caller's roleIds.
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
