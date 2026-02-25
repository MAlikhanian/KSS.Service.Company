using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using KSS.Entity;

namespace KSS.Data.Configuration
{
    public class CompanyTypeConfiguration : IEntityTypeConfiguration<CompanyType>
    {
        public void Configure(EntityTypeBuilder<CompanyType> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.CompanyType).HasForeignKey(x => x.CompanyTypeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Companies).WithOne(x => x.CompanyType).HasForeignKey(x => x.CompanyTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CompanyTypeTranslationConfiguration : IEntityTypeConfiguration<CompanyTypeTranslation>
    {
        public void Configure(EntityTypeBuilder<CompanyTypeTranslation> b)
        {
            b.HasKey(x => new { x.CompanyTypeId, x.LanguageId });
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
            
            b.HasOne(x => x.CompanyType).WithMany(x => x.Companies).HasForeignKey(x => x.CompanyTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Industry).WithMany(x => x.Companies).HasForeignKey(x => x.IndustryId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Translations).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.NameHistories).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Stakeholders).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Emails).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Phones).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Addresses).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CompanyTranslationConfiguration : IEntityTypeConfiguration<CompanyTranslation>
    {
        public void Configure(EntityTypeBuilder<CompanyTranslation> b)
        {
            b.HasKey(x => new { x.CompanyId, x.LanguageId });
        }
    }

    public class CompanyNameHistoryConfiguration : IEntityTypeConfiguration<CompanyNameHistory>
    {
        public void Configure(EntityTypeBuilder<CompanyNameHistory> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_CompanyNameHistory_SetUpdatedAt, TR_CompanyNameHistory_PreventOverlap)
            b.ToTable(tb => tb.UseSqlOutputClause(false));
            
            b.HasMany(x => x.Translations).WithOne(x => x.NameHistory).HasForeignKey(x => x.CompanyNameHistoryId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CompanyNameHistoryTranslationConfiguration : IEntityTypeConfiguration<CompanyNameHistoryTranslation>
    {
        public void Configure(EntityTypeBuilder<CompanyNameHistoryTranslation> b)
        {
            b.HasKey(x => new { x.CompanyNameHistoryId, x.LanguageId });
        }
    }

    public class CompanyFinancialInfoConfiguration : IEntityTypeConfiguration<CompanyFinancialInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyFinancialInfo> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_CompanyFinancialInfo_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));

            // Unique constraint: one record per company per fiscal year
            b.HasIndex(x => new { x.CompanyId, x.FiscalYear }).IsUnique();
        }
    }

    public class CompanyStakeholderConfiguration : IEntityTypeConfiguration<CompanyStakeholder>
    {
        public void Configure(EntityTypeBuilder<CompanyStakeholder> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_CompanyStakeholder_SetUpdatedAt)
            b.ToTable(tb => tb.UseSqlOutputClause(false));
            
            b.HasOne(x => x.StakeholderType).WithMany(x => x.Stakeholders).HasForeignKey(x => x.StakeholderTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Histories).WithOne(x => x.Stakeholder).HasForeignKey(x => x.CompanyStakeholderId).OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint: (CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId)
            b.HasIndex(x => new { x.CompanyId, x.StakeholderTypeId, x.RelatedPartyType, x.RelatedPartyId }).IsUnique();
        }
    }

    public class CompanyStakeholderHistoryConfiguration : IEntityTypeConfiguration<CompanyStakeholderHistory>
    {
        public void Configure(EntityTypeBuilder<CompanyStakeholderHistory> b)
        {
            // Disable OUTPUT clause because table has triggers (TR_CompanyStakeholderHistory_SetUpdatedAt, TR_CompanyStakeholderHistory_PreventOverlap)
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
}
