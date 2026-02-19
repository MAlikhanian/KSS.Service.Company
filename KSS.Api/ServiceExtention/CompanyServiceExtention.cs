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
            var connectionString = configuration.GetSection("ConnectionStrings")["KSSCompany"]
                ?? configuration.GetSection("ConnectionStrings")["KSSMain"];

            services.AddDbContext<MainDbContext>(options => options.UseSqlServer(connectionString));

            // Lookup services
            services.AddScoped<ICompanyTypeRepository, CompanyTypeRepository>();
            services.AddScoped<ICompanyTypeService, CompanyTypeService>();
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
            services.AddScoped<ICompanyTranslationRepository, CompanyTranslationRepository>();
            services.AddScoped<ICompanyTranslationService, CompanyTranslationService>();
            services.AddScoped<ICompanyNameHistoryRepository, CompanyNameHistoryRepository>();
            services.AddScoped<ICompanyNameHistoryService, CompanyNameHistoryService>();
            services.AddScoped<ICompanyStakeholderRepository, CompanyStakeholderRepository>();
            services.AddScoped<ICompanyStakeholderService, CompanyStakeholderService>();
            services.AddScoped<ICompanyStakeholderHistoryRepository, CompanyStakeholderHistoryRepository>();
            services.AddScoped<ICompanyStakeholderHistoryService, CompanyStakeholderHistoryService>();

            // Contact data services
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPhoneRepository, PhoneRepository>();
            services.AddScoped<IPhoneService, PhoneService>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAddressTranslationRepository, AddressTranslationRepository>();
            services.AddScoped<IAddressTranslationService, AddressTranslationService>();

            return services;
        }
    }
}
