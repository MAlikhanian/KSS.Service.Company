using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using KSS.Entity;

namespace KSS.Data.Configuration
{
    public class LegalFormConfiguration : IEntityTypeConfiguration<LegalForm>
    {
        public void Configure(EntityTypeBuilder<LegalForm> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.LegalForm).HasForeignKey(x => x.LegalFormId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Companies).WithOne(x => x.LegalForm).HasForeignKey(x => x.LegalFormId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class LegalFormTranslationConfiguration : IEntityTypeConfiguration<LegalFormTranslation>
    {
        public void Configure(EntityTypeBuilder<LegalFormTranslation> b)
        {
            b.HasKey(x => new { x.LegalFormId, x.LanguageId });
        }
    }

    public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.Industry).HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Companies).WithOne(x => x.Industry).HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class IndustryTranslationConfiguration : IEntityTypeConfiguration<IndustryTranslation>
    {
        public void Configure(EntityTypeBuilder<IndustryTranslation> b)
        {
            b.HasKey(x => new { x.IndustryId, x.LanguageId });
        }
    }

    public class StakeholderTypeConfiguration : IEntityTypeConfiguration<StakeholderType>
    {
        public void Configure(EntityTypeBuilder<StakeholderType> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.StakeholderType).HasForeignKey(x => x.StakeholderTypeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Stakeholders).WithOne(x => x.StakeholderType).HasForeignKey(x => x.StakeholderTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class StakeholderTypeTranslationConfiguration : IEntityTypeConfiguration<StakeholderTypeTranslation>
    {
        public void Configure(EntityTypeBuilder<StakeholderTypeTranslation> b)
        {
            b.HasKey(x => new { x.StakeholderTypeId, x.LanguageId });
        }
    }

    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_Company_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasOne(x => x.LegalForm).WithMany(x => x.Companies).HasForeignKey(x => x.LegalFormId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Industry).WithMany(x => x.Companies).HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Translations).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.NameHistories).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Stakeholders).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Emails).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Phones).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Addresses).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Documents).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Websites).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.CompanySoftwares).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
    {
        public void Configure(EntityTypeBuilder<Translation> b)
        {
            b.HasKey(x => new { x.CompanyId, x.LanguageId });
        }
    }

    public class NameHistoryConfiguration : IEntityTypeConfiguration<NameHistory>
    {
        public void Configure(EntityTypeBuilder<NameHistory> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_NameHistory_SetUpdatedAt, TR_NameHistory_PreventOverlap)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasMany(x => x.Translations).WithOne(x => x.NameHistory).HasForeignKey(x => x.NameHistoryId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class NameHistoryTranslationConfiguration : IEntityTypeConfiguration<NameHistoryTranslation>
    {
        public void Configure(EntityTypeBuilder<NameHistoryTranslation> b)
        {
            b.HasKey(x => new { x.NameHistoryId, x.LanguageId });
        }
    }

    public class FinancialInfoConfiguration : IEntityTypeConfiguration<FinancialInfo>
    {
        public void Configure(EntityTypeBuilder<FinancialInfo> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_FinancialInfo_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            // Unique constraint: one record per company per fiscal year
            b.HasIndex(x => new { x.CompanyId, x.FiscalYear }).IsUnique();
        }
    }

    public class StakeholderConfiguration : IEntityTypeConfiguration<Stakeholder>
    {
        public void Configure(EntityTypeBuilder<Stakeholder> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_Stakeholder_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasOne(x => x.StakeholderType).WithMany(x => x.Stakeholders).HasForeignKey(x => x.StakeholderTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Histories).WithOne(x => x.Stakeholder).HasForeignKey(x => x.CompanyStakeholderId).OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: (CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId)
            b.HasIndex(x => new { x.CompanyId, x.StakeholderTypeId, x.RelatedPartyType, x.RelatedPartyId }).IsUnique();
        }
    }

    public class StakeholderHistoryConfiguration : IEntityTypeConfiguration<StakeholderHistory>
    {
        public void Configure(EntityTypeBuilder<StakeholderHistory> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_StakeholderHistory_SetUpdatedAt, TR_StakeholderHistory_PreventOverlap)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            // Unique constraint: (CompanyStakeholderId, EffectiveDate)
            b.HasIndex(x => new { x.CompanyStakeholderId, x.EffectiveDate }).IsUnique();
        }
    }

    // Update Email configuration to use Company
    public class EmailConfiguration : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_Email_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasOne(x => x.Label).WithMany(x => x.Emails).HasForeignKey(x => x.LabelId).OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: (CompanyId, Email)
            b.HasIndex(x => new { x.CompanyId, x.EmailAddress }).IsUnique();

            // Unique filtered index: one primary per company
            b.HasIndex(x => x.CompanyId).IsUnique().HasFilter("[IsPrimary] = 1");
        }
    }

    // Update Phone configuration to use Company
    public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
    {
        public void Configure(EntityTypeBuilder<Phone> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_Phone_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasOne(x => x.Label).WithMany(x => x.Phones).HasForeignKey(x => x.LabelId).OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: (CompanyId, CountryId, PhoneNumber)
            b.HasIndex(x => new { x.CompanyId, x.CountryId, x.PhoneNumber }).IsUnique();

            // Unique filtered index: one primary per company
            b.HasIndex(x => x.CompanyId).IsUnique().HasFilter("[IsPrimary] = 1");
        }
    }

    // Update Address configuration to use Company
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_Address_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            b.HasOne(x => x.Label).WithMany(x => x.Addresses).HasForeignKey(x => x.LabelId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Translations).WithOne(x => x.Address).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Cascade);

            // Unique filtered index: one primary per company
            b.HasIndex(x => x.CompanyId).IsUnique().HasFilter("[IsPrimary] = 1");
        }
    }

    // Label configurations for Company contact data (Email, Phone, Address)
    public class EmailLabelConfiguration : IEntityTypeConfiguration<EmailLabel>
    {
        public void Configure(EntityTypeBuilder<EmailLabel> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.EmailLabel).HasForeignKey(x => x.EmailLabelId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class EmailLabelTranslationConfiguration : IEntityTypeConfiguration<EmailLabelTranslation>
    {
        public void Configure(EntityTypeBuilder<EmailLabelTranslation> b)
        {
            b.HasKey(x => new { x.EmailLabelId, x.LanguageId });
        }
    }

    public class PhoneLabelConfiguration : IEntityTypeConfiguration<PhoneLabel>
    {
        public void Configure(EntityTypeBuilder<PhoneLabel> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.PhoneLabel).HasForeignKey(x => x.PhoneLabelId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PhoneLabelTranslationConfiguration : IEntityTypeConfiguration<PhoneLabelTranslation>
    {
        public void Configure(EntityTypeBuilder<PhoneLabelTranslation> b)
        {
            b.HasKey(x => new { x.PhoneLabelId, x.LanguageId });
        }
    }

    public class AddressLabelConfiguration : IEntityTypeConfiguration<AddressLabel>
    {
        public void Configure(EntityTypeBuilder<AddressLabel> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.AddressLabel).HasForeignKey(x => x.AddressLabelId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AddressLabelTranslationConfiguration : IEntityTypeConfiguration<AddressLabelTranslation>
    {
        public void Configure(EntityTypeBuilder<AddressLabelTranslation> b)
        {
            b.HasKey(x => new { x.AddressLabelId, x.LanguageId });
        }
    }

    public class AddressTranslationConfiguration : IEntityTypeConfiguration<AddressTranslation>
    {
        public void Configure(EntityTypeBuilder<AddressTranslation> b)
        {
            b.HasKey(x => new { x.AddressId, x.LanguageId });
        }
    }

    public class WebsiteConfiguration : IEntityTypeConfiguration<Website>
    {
        public void Configure(EntityTypeBuilder<Website> b)
        {
            b.HasOne(x => x.Label).WithMany(x => x.Websites).HasForeignKey(x => x.LabelId).OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: (CompanyId, Url) — no duplicate URL per company
            b.HasIndex(x => new { x.CompanyId, x.Url }).IsUnique();

            // Unique filtered index: one primary per company
            b.HasIndex(x => x.CompanyId).IsUnique().HasFilter("[IsPrimary] = 1");
        }
    }

    public class WebsiteLabelConfiguration : IEntityTypeConfiguration<WebsiteLabel>
    {
        public void Configure(EntityTypeBuilder<WebsiteLabel> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.WebsiteLabel).HasForeignKey(x => x.WebsiteLabelId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class WebsiteLabelTranslationConfiguration : IEntityTypeConfiguration<WebsiteLabelTranslation>
    {
        public void Configure(EntityTypeBuilder<WebsiteLabelTranslation> b)
        {
            b.HasKey(x => new { x.WebsiteLabelId, x.LanguageId });
        }
    }

    public class CompanySoftwareConfiguration : IEntityTypeConfiguration<CompanySoftware>
    {
        public void Configure(EntityTypeBuilder<CompanySoftware> b)
        {
            b.HasOne(x => x.Category).WithMany(x => x.CompanySoftwares).HasForeignKey(x => x.SoftwareCategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Software).WithMany(x => x.CompanySoftwares).HasForeignKey(x => x.SoftwareId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.CompanyId, x.SoftwareCategoryId }).IsUnique();
        }
    }

    public class SoftwareCategoryConfiguration : IEntityTypeConfiguration<SoftwareCategory>
    {
        public void Configure(EntityTypeBuilder<SoftwareCategory> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.SoftwareCategory).HasForeignKey(x => x.SoftwareCategoryId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SoftwareCategoryTranslationConfiguration : IEntityTypeConfiguration<SoftwareCategoryTranslation>
    {
        public void Configure(EntityTypeBuilder<SoftwareCategoryTranslation> b)
        {
            b.HasKey(x => new { x.SoftwareCategoryId, x.LanguageId });
        }
    }

    public class SoftwareConfiguration : IEntityTypeConfiguration<Software>
    {
        public void Configure(EntityTypeBuilder<Software> b)
        {
            // Software name is unique PER PROVIDER — two providers can each have a
            // same-named product; the cascade (provider→software) disambiguates.
            b.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            // Provider company. NO ACTION to avoid a multiple-cascade-path conflict
            // with CompanySoftware.CompanyId (cascade). No inverse nav on Company.
            b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            b.HasIndex(x => x.CompanyId);
        }
    }

    // Company documents (metadata) + their type lookup/translation.
    public class CompanyDocumentConfiguration : IEntityTypeConfiguration<CompanyDocument>
    {
        public void Configure(EntityTypeBuilder<CompanyDocument> b)
        {
            // Restrict delete on the type — a document type in use can't be removed.
            b.HasOne(x => x.CompanyDocumentType).WithMany(x => x.Documents).HasForeignKey(x => x.CompanyDocumentTypeId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CompanyId);
        }
    }

    public class CompanyDocumentTypeConfiguration : IEntityTypeConfiguration<CompanyDocumentType>
    {
        public void Configure(EntityTypeBuilder<CompanyDocumentType> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.CompanyDocumentType).HasForeignKey(x => x.CompanyDocumentTypeId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CompanyDocumentTypeTranslationConfiguration : IEntityTypeConfiguration<CompanyDocumentTypeTranslation>
    {
        public void Configure(EntityTypeBuilder<CompanyDocumentTypeTranslation> b)
        {
            b.HasKey(x => new { x.CompanyDocumentTypeId, x.LanguageId });
        }
    }

}
