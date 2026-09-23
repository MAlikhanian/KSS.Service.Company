using Microsoft.EntityFrameworkCore;
using KSS.Entity;

namespace KSS.Data.DbContexts
{
    public partial class MainDbContext
    {
        // Lookup tables
        public DbSet<LegalForm> LegalForms { get; set; }
        public DbSet<LegalFormTranslation> LegalFormTranslations { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<IndustryTranslation> IndustryTranslations { get; set; }
        public DbSet<StakeholderType> StakeholderTypes { get; set; }
        public DbSet<StakeholderTypeTranslation> StakeholderTypeTranslations { get; set; }
        public DbSet<EmailLabel> EmailLabels { get; set; }
        public DbSet<EmailLabelTranslation> EmailLabelTranslations { get; set; }
        public DbSet<PhoneLabel> PhoneLabels { get; set; }
        public DbSet<PhoneLabelTranslation> PhoneLabelTranslations { get; set; }
        public DbSet<AddressLabel> AddressLabels { get; set; }
        public DbSet<AddressLabelTranslation> AddressLabelTranslations { get; set; }
        public DbSet<WebsiteLabel> WebsiteLabels { get; set; }
        public DbSet<WebsiteLabelTranslation> WebsiteLabelTranslations { get; set; }
        public DbSet<SoftwareCategory> SoftwareCategories { get; set; }
        public DbSet<SoftwareCategoryTranslation> SoftwareCategoryTranslations { get; set; }
        public DbSet<Software> Softwares { get; set; }
        public DbSet<CompanyDocumentType> CompanyDocumentTypes { get; set; }
        public DbSet<CompanyDocumentTypeTranslation> CompanyDocumentTypeTranslations { get; set; }

        // Main entities
        public DbSet<Company> Companies { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<NameHistory> NameHistories { get; set; }
        public DbSet<NameHistoryTranslation> NameHistoryTranslations { get; set; }
        public DbSet<Stakeholder> Stakeholders { get; set; }
        public DbSet<StakeholderHistory> StakeholderHistories { get; set; }
        public DbSet<FinancialInfo> FinancialInfos { get; set; }

        // Contact data
        public DbSet<Email> Emails { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressTranslation> AddressTranslations { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<CompanySoftware> CompanySoftwares { get; set; }

        // Documents (metadata only — bytes live in FileStorage via FileOrchestrator)
        public DbSet<CompanyDocument> CompanyDocuments { get; set; }
    }
}
