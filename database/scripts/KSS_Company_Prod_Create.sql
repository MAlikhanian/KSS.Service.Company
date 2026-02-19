-- ============================================================
-- Database: KSS_Company_Prod (microservice: company)
-- Table order follows dependencies for readability:
--   Section 1: Lookups (CompanyType, Industry, StakeholderType, EmailLabel, PhoneLabel, AddressLabel) + their *Translation tables
--   Section 2: Company (depends on CompanyType, Industry)
--   Section 3: CompanyTranslation, CompanyNameHistory + CompanyNameHistoryTranslation
--   Section 4: CompanyStakeholder, CompanyStakeholderHistory
--   Section 5: Email, Phone, Address, AddressTranslation
-- LanguageId ref KSS_Common_Prod.dbo.[Language]; CountryId/RegionId/CityId ref KSS_Common_Prod (no FK cross-database).
-- Note: This script assumes the database is dropped before running (fresh database each time).
-- ============================================================

-- Safety check: Stop execution if database already exists
IF DB_ID(N'KSS_Company_Prod') IS NOT NULL
BEGIN
    RAISERROR('Database KSS_Company_Prod already exists. Please drop the database before running this script.', 16, 1);
    RETURN;
END
GO

CREATE DATABASE [KSS_Company_Prod];
GO

USE [KSS_Company_Prod];
GO

-- ============================================================
-- SECTION 1: Lookup tables (no FK to Company; define first)
-- ============================================================

-- CompanyType + CompanyTypeTranslation
-- ============================================================
CREATE TABLE dbo.[CompanyType] (
    Id     TINYINT      IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(20)  NOT NULL, -- کد
    CONSTRAINT PK_CompanyType PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CompanyType_Code UNIQUE (Code)
);
CREATE TABLE dbo.[CompanyTypeTranslation] (
    CompanyTypeId TINYINT      NOT NULL, -- شناسه نوع شرکت
    LanguageId    SMALLINT     NOT NULL, -- شناسه زبان
    Name          NVARCHAR(50) NOT NULL, -- نام
    CONSTRAINT PK_CompanyTypeTranslation PRIMARY KEY CLUSTERED (CompanyTypeId, LanguageId),
    CONSTRAINT FK_CompanyTypeTranslation_CompanyType FOREIGN KEY (CompanyTypeId) REFERENCES dbo.[CompanyType] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_CompanyTypeTranslation_LanguageId ON dbo.[CompanyTypeTranslation] (LanguageId);
GO

-- Industry + IndustryTranslation
-- ============================================================
CREATE TABLE dbo.[Industry] (
    Id     SMALLINT     IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(20)  NOT NULL, -- کد
    CONSTRAINT PK_Industry PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Industry_Code UNIQUE (Code)
);
CREATE TABLE dbo.[IndustryTranslation] (
    IndustryId SMALLINT     NOT NULL, -- شناسه صنعت
    LanguageId SMALLINT     NOT NULL, -- شناسه زبان
    Name       NVARCHAR(80) NOT NULL, -- نام
    CONSTRAINT PK_IndustryTranslation PRIMARY KEY CLUSTERED (IndustryId, LanguageId),
    CONSTRAINT FK_IndustryTranslation_Industry FOREIGN KEY (IndustryId) REFERENCES dbo.[Industry] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_IndustryTranslation_LanguageId ON dbo.[IndustryTranslation] (LanguageId);
GO

-- StakeholderType + StakeholderTypeTranslation
-- ============================================================
CREATE TABLE dbo.[StakeholderType] (
    Id     TINYINT      IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(20)  NOT NULL, -- کد
    CONSTRAINT PK_StakeholderType PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_StakeholderType_Code UNIQUE (Code)
);
CREATE TABLE dbo.[StakeholderTypeTranslation] (
    StakeholderTypeId TINYINT       NOT NULL, -- شناسه نوع ذینفع
    LanguageId        SMALLINT     NOT NULL, -- شناسه زبان
    Name               NVARCHAR(50) NOT NULL, -- نام
    CONSTRAINT PK_StakeholderTypeTranslation PRIMARY KEY CLUSTERED (StakeholderTypeId, LanguageId),
    CONSTRAINT FK_StakeholderTypeTranslation_Type FOREIGN KEY (StakeholderTypeId) REFERENCES dbo.[StakeholderType] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_StakeholderTypeTranslation_LanguageId ON dbo.[StakeholderTypeTranslation] (LanguageId);
GO

-- EmailLabel + EmailLabelTranslation
-- ============================================================
CREATE TABLE dbo.[EmailLabel] (
    Id     TINYINT       IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(10)   NOT NULL, -- کد
    CONSTRAINT PK_EmailLabel PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_EmailLabel_Code UNIQUE (Code)
);
CREATE TABLE dbo.[EmailLabelTranslation] (
    EmailLabelId TINYINT       NOT NULL, -- شناسه برچسب ایمیل
    LanguageId   SMALLINT      NOT NULL, -- شناسه زبان
    Name         NVARCHAR(50)  NOT NULL, -- نام
    CONSTRAINT PK_EmailLabelTranslation PRIMARY KEY CLUSTERED (EmailLabelId, LanguageId),
    CONSTRAINT FK_EmailLabelTranslation_EmailLabel FOREIGN KEY (EmailLabelId) REFERENCES dbo.[EmailLabel] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_EmailLabelTranslation_LanguageId ON dbo.[EmailLabelTranslation] (LanguageId);
GO

-- PhoneLabel + PhoneLabelTranslation
-- ============================================================
CREATE TABLE dbo.[PhoneLabel] (
    Id     TINYINT       IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(10)   NOT NULL, -- کد
    CONSTRAINT PK_PhoneLabel PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PhoneLabel_Code UNIQUE (Code)
);
CREATE TABLE dbo.[PhoneLabelTranslation] (
    PhoneLabelId TINYINT       NOT NULL, -- شناسه برچسب تلفن
    LanguageId   SMALLINT      NOT NULL, -- شناسه زبان
    Name         NVARCHAR(50)  NOT NULL, -- نام
    CONSTRAINT PK_PhoneLabelTranslation PRIMARY KEY CLUSTERED (PhoneLabelId, LanguageId),
    CONSTRAINT FK_PhoneLabelTranslation_PhoneLabel FOREIGN KEY (PhoneLabelId) REFERENCES dbo.[PhoneLabel] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_PhoneLabelTranslation_LanguageId ON dbo.[PhoneLabelTranslation] (LanguageId);
GO

-- AddressLabel + AddressLabelTranslation
-- ============================================================
CREATE TABLE dbo.[AddressLabel] (
    Id     TINYINT       IDENTITY(1, 1) NOT NULL, -- شناسه
    Code   VARCHAR(10)   NOT NULL, -- کد
    CONSTRAINT PK_AddressLabel PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_AddressLabel_Code UNIQUE (Code)
);
CREATE TABLE dbo.[AddressLabelTranslation] (
    AddressLabelId TINYINT      NOT NULL, -- شناسه برچسب آدرس
    LanguageId     SMALLINT     NOT NULL, -- شناسه زبان
    Name           NVARCHAR(50) NOT NULL, -- نام
    CONSTRAINT PK_AddressLabelTranslation PRIMARY KEY CLUSTERED (AddressLabelId, LanguageId),
    CONSTRAINT FK_AddressLabelTranslation_AddressLabel FOREIGN KEY (AddressLabelId) REFERENCES dbo.[AddressLabel] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_AddressLabelTranslation_LanguageId ON dbo.[AddressLabelTranslation] (LanguageId);
GO

-- ============================================================
-- SECTION 2: Company (main entity; depends on CompanyType, Industry)
-- Registration: تاریخ ثبت (RegistrationDate), شماره ثبت (RegistrationNo), شناسه ملی (NationalId),
-- کد اقتصادی (EconomicCode), استان ثبت/محل ثبت (RegistrationCountryId, RegistrationRegionId, RegistrationCityId)
-- ============================================================
CREATE TABLE dbo.[Company] (
    Id                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Company_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyTypeId           TINYINT          NOT NULL, -- شناسه نوع شرکت
    IndustryId              SMALLINT         NULL, -- Industry. NULL = unknown at creation (often set after verification/onboarding). If your domain always requires industry: make NOT NULL; or add Industry.Code='Unknown' and default to it.
    RegistrationDate        DATE             NOT NULL, -- تاریخ ثبت
    RegistrationNo          VARCHAR(30)      NOT NULL, -- شماره ثبت
    NationalId              VARCHAR(20)      NOT NULL, -- شناسه ملی
    EconomicCode            VARCHAR(20)      NOT NULL, -- کد اقتصادی
    RegistrationCountryId   SMALLINT         NOT NULL, -- کشور (ref Common)
    RegistrationRegionId    SMALLINT         NOT NULL, -- استان / منطقه
    RegistrationCityId      INT              NOT NULL, -- شهر
    TaxId                   VARCHAR(30)      NULL, -- شناسه مالیاتی
    FoundedDate             DATE             NULL, -- تاریخ تأسیس
    Website                 VARCHAR(256)     NULL, -- وب‌سایت
    LogoUrl                 VARCHAR(512)     NULL, -- آدرس لوگو
    IsActive                BIT              NOT NULL CONSTRAINT DF_Company_IsActive DEFAULT 1, -- فعال
    CreatedAt               DATETIME2(7)     NOT NULL CONSTRAINT DF_Company_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt               DATETIME2(7)     NOT NULL CONSTRAINT DF_Company_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_Company PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Company_CompanyType FOREIGN KEY (CompanyTypeId) REFERENCES dbo.[CompanyType] (Id),
    CONSTRAINT FK_Company_Industry FOREIGN KEY (IndustryId) REFERENCES dbo.[Industry] (Id)
);
GO
CREATE NONCLUSTERED INDEX IX_Company_CompanyTypeId ON dbo.[Company] (CompanyTypeId);
CREATE NONCLUSTERED INDEX IX_Company_IndustryId ON dbo.[Company] (IndustryId) WHERE IndustryId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Company_RegistrationDate ON dbo.[Company] (RegistrationDate);
-- Multi-country: uniqueness per registration country (columns NOT NULL, no filter needed)
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_RegistrationNo_Country ON dbo.[Company] (RegistrationCountryId, RegistrationNo);
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_NationalId_Country ON dbo.[Company] (RegistrationCountryId, NationalId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_EconomicCode_Country ON dbo.[Company] (RegistrationCountryId, EconomicCode);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationCountryId ON dbo.[Company] (RegistrationCountryId);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationRegionId ON dbo.[Company] (RegistrationRegionId);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationCityId ON dbo.[Company] (RegistrationCityId);
CREATE NONCLUSTERED INDEX IX_Company_TaxId ON dbo.[Company] (TaxId) WHERE TaxId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Company_IsActive ON dbo.[Company] (IsActive) WHERE IsActive = 1;
GO

-- ============================================================
-- SECTION 3: Company direct children (depend on Company only)
-- ============================================================

-- CompanyTranslation (current name/description per language)
-- ============================================================
CREATE TABLE dbo.[CompanyTranslation] (
    CompanyId   UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    LanguageId  SMALLINT         NOT NULL, -- شناسه زبان
    Name        NVARCHAR(150)    NOT NULL, -- نام
    ShortName   NVARCHAR(50)     NULL, -- نام کوتاه
    Description NVARCHAR(500)    NULL, -- توضیحات
    CONSTRAINT PK_CompanyTranslation PRIMARY KEY CLUSTERED (CompanyId, LanguageId),
    CONSTRAINT FK_CompanyTranslation_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_CompanyTranslation_Name ON dbo.[CompanyTranslation] (Name);
CREATE NONCLUSTERED INDEX IX_CompanyTranslation_LanguageId_Name ON dbo.[CompanyTranslation] (LanguageId, Name); -- covers LanguageId-only queries too
GO

-- CompanyNameHistory + CompanyNameHistoryTranslation (name change history)
-- ============================================================
CREATE TABLE dbo.[CompanyNameHistory] (
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CompanyNameHistory_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyId    UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    StartDate    DATE             NOT NULL, -- تاریخ شروع
    EndDate      DATE             NULL, -- تاریخ پایان (NULL = جاری)
    CreatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyNameHistory_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyNameHistory_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_CompanyNameHistory PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CompanyNameHistory_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE,
    CONSTRAINT CK_CompanyNameHistory_DateRange CHECK (EndDate IS NULL OR StartDate <= EndDate)
);
GO
CREATE NONCLUSTERED INDEX IX_CompanyNameHistory_CompanyId ON dbo.[CompanyNameHistory] (CompanyId);
CREATE NONCLUSTERED INDEX IX_CompanyNameHistory_StartDate ON dbo.[CompanyNameHistory] (StartDate);
CREATE NONCLUSTERED INDEX IX_CompanyNameHistory_EndDate ON dbo.[CompanyNameHistory] (EndDate) WHERE EndDate IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UX_CompanyNameHistory_Current ON dbo.[CompanyNameHistory] (CompanyId) WHERE EndDate IS NULL;
GO
CREATE TABLE dbo.[CompanyNameHistoryTranslation] (
    CompanyNameHistoryId UNIQUEIDENTIFIER NOT NULL, -- شناسه تاریخچه نام شرکت
    LanguageId           SMALLINT        NOT NULL, -- شناسه زبان
    Name                 NVARCHAR(150)   NOT NULL, -- نام
    ShortName            NVARCHAR(50)    NULL, -- نام کوتاه
    CONSTRAINT PK_CompanyNameHistoryTranslation PRIMARY KEY CLUSTERED (CompanyNameHistoryId, LanguageId),
    CONSTRAINT FK_CompanyNameHistoryTranslation_History FOREIGN KEY (CompanyNameHistoryId) REFERENCES dbo.[CompanyNameHistory] (Id) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX IX_CompanyNameHistoryTranslation_LanguageId ON dbo.[CompanyNameHistoryTranslation] (LanguageId);
CREATE NONCLUSTERED INDEX IX_CompanyNameHistoryTranslation_Name ON dbo.[CompanyNameHistoryTranslation] (Name);
GO

-- ============================================================
-- SECTION 4: CompanyStakeholder (depends on Company, StakeholderType)
-- ============================================================
-- CompanyStakeholder: CompanyId has StakeholderType with RelatedParty (polymorphic: Company or Person)
-- RelatedPartyType: 1 = Company, 2 = Person (integrity enforced in app; no cross-DB FK)
-- Note: All changeable fields tracked in CompanyStakeholderHistory
-- ============================================================
CREATE TABLE dbo.[CompanyStakeholder] (
    Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CompanyStakeholder_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyId            UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    RelatedPartyType     TINYINT          NOT NULL, -- نوع طرف مرتبط (۱=شرکت، ۲=شخص)
    RelatedPartyId       UNIQUEIDENTIFIER NOT NULL, -- شناسه طرف مرتبط
    StakeholderTypeId    TINYINT          NOT NULL, -- شناسه نوع ذینفع
    CreatedAt            DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyStakeholder_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt            DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyStakeholder_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_CompanyStakeholder PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CompanyStakeholder_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_CompanyStakeholder_Type FOREIGN KEY (StakeholderTypeId) REFERENCES dbo.[StakeholderType] (Id),
    CONSTRAINT UQ_CompanyStakeholder UNIQUE (CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId),
    CONSTRAINT CK_CompanyStakeholder_NoSelfCompany CHECK (RelatedPartyType <> 1 OR CompanyId <> RelatedPartyId),
    CONSTRAINT CK_CompanyStakeholder_RelatedPartyType CHECK (RelatedPartyType IN (1, 2)) -- 1=Company, 2=Person
);
GO
CREATE NONCLUSTERED INDEX IX_CompanyStakeholder_CompanyId ON dbo.[CompanyStakeholder] (CompanyId);
CREATE NONCLUSTERED INDEX IX_CompanyStakeholder_RelatedParty ON dbo.[CompanyStakeholder] (RelatedPartyType, RelatedPartyId);
CREATE NONCLUSTERED INDEX IX_CompanyStakeholder_StakeholderTypeId ON dbo.[CompanyStakeholder] (StakeholderTypeId);
GO

-- CompanyStakeholderHistory (depends on CompanyStakeholder)
-- ============================================================
-- CompanyStakeholderHistory: History table for tracking changes of stakeholder fields
-- Tracks changes for: درصد مالکیت (OwnershipPercentage), تعداد سهام (ShareCount),
-- نماینده هیئت مدیره (BoardRepresentativePersonId), تاریخ ثبت (RegistrationDate)
-- ============================================================
CREATE TABLE dbo.[CompanyStakeholderHistory] (
    Id                           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CompanyStakeholderHistory_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyStakeholderId         UNIQUEIDENTIFIER NOT NULL, -- شناسه ذینفع شرکت
    OwnershipPercentage          DECIMAL(5, 2)   NOT NULL, -- درصد مالکیت
    ShareCount                   BIGINT           NOT NULL, -- تعداد سهام
    BoardRepresentativePersonId  UNIQUEIDENTIFIER NULL, -- شناسه نماینده هیئت مدیره (اختیاری)
    RegistrationDate             DATE             NOT NULL, -- تاریخ ثبت (official legal filing date)
    EffectiveDate                DATE             NOT NULL, -- تاریخ اعمال تغییرات (when change takes effect; may differ from RegistrationDate)
    EndDate                      DATE             NULL, -- تاریخ پایان اعتبار (NULL = جاری)
    CreatedAt                    DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyStakeholderHistory_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt                    DATETIME2(7)     NOT NULL CONSTRAINT DF_CompanyStakeholderHistory_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_CompanyStakeholderHistory PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CompanyStakeholderHistory_Stakeholder FOREIGN KEY (CompanyStakeholderId) REFERENCES dbo.[CompanyStakeholder] (Id) ON DELETE CASCADE,
    CONSTRAINT CK_CompanyStakeholderHistory_OwnershipPercentage CHECK (OwnershipPercentage >= 0 AND OwnershipPercentage <= 100),
    CONSTRAINT CK_CompanyStakeholderHistory_ShareCount CHECK (ShareCount >= 0),
    CONSTRAINT CK_CompanyStakeholderHistory_DateRange CHECK (EndDate IS NULL OR EffectiveDate <= EndDate)
);
GO
CREATE NONCLUSTERED INDEX IX_CompanyStakeholderHistory_CompanyStakeholderId ON dbo.[CompanyStakeholderHistory] (CompanyStakeholderId);
CREATE NONCLUSTERED INDEX IX_CompanyStakeholderHistory_BoardRepresentativePersonId ON dbo.[CompanyStakeholderHistory] (BoardRepresentativePersonId) WHERE BoardRepresentativePersonId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_CompanyStakeholderHistory_EffectiveDate ON dbo.[CompanyStakeholderHistory] (EffectiveDate);
CREATE NONCLUSTERED INDEX IX_CompanyStakeholderHistory_EndDate ON dbo.[CompanyStakeholderHistory] (EndDate) WHERE EndDate IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_CompanyStakeholderHistory_RegistrationDate ON dbo.[CompanyStakeholderHistory] (RegistrationDate);
CREATE UNIQUE NONCLUSTERED INDEX UX_CompanyStakeholderHistory_Current ON dbo.[CompanyStakeholderHistory] (CompanyStakeholderId) WHERE EndDate IS NULL;
CREATE UNIQUE NONCLUSTERED INDEX UX_CompanyStakeholderHistory_Stakeholder_EffectiveDate ON dbo.[CompanyStakeholderHistory] (CompanyStakeholderId, EffectiveDate);
GO

-- ============================================================
-- SECTION 5: Contact data (depend on Company + labels from Section 1)
-- ============================================================

-- Email (depends on Company, EmailLabel)
-- ============================================================
CREATE TABLE dbo.[Email] (
    Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Email_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyId      UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    LabelId        TINYINT          NOT NULL, -- شناسه برچسب
    Email          VARCHAR(128)     COLLATE Latin1_General_100_CI_AS NOT NULL, -- ایمیل (Collation Latin1_General_100_CI_AS: case-insensitive، accent-sensitive)
    IsPrimary      BIT              NOT NULL CONSTRAINT DF_Email_IsPrimary DEFAULT 0, -- اصلی
    IsVerified     BIT              NOT NULL CONSTRAINT DF_Email_IsVerified DEFAULT 0, -- تأیید شده
    VerifiedAt     DATETIME2(7)     NULL, -- تاریخ تأیید (الزامی وقتی IsVerified=1)
    CreatedAt      DATETIME2(7)     NOT NULL CONSTRAINT DF_Email_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt      DATETIME2(7)     NOT NULL CONSTRAINT DF_Email_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_Email PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Email_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Email_Label FOREIGN KEY (LabelId) REFERENCES dbo.[EmailLabel] (Id),
    CONSTRAINT CK_Email_VerifiedAt CHECK (IsVerified = 0 OR VerifiedAt IS NOT NULL),
    CONSTRAINT CK_Email_Format CHECK (Email LIKE '%_@_%._%' AND Email NOT LIKE '% %') -- حداقل فرمت و بدون فاصله
);
CREATE NONCLUSTERED INDEX IX_Email_CompanyId ON dbo.[Email] (CompanyId);
CREATE NONCLUSTERED INDEX IX_Email_LabelId ON dbo.[Email] (LabelId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Email_Company_Email ON dbo.[Email] (CompanyId, Email);
CREATE NONCLUSTERED INDEX IX_Email_Email ON dbo.[Email] (Email);
CREATE UNIQUE NONCLUSTERED INDEX UX_Email_Company_Primary ON dbo.[Email] (CompanyId) WHERE IsPrimary = 1;
GO

-- Phone (depends on Company, PhoneLabel)
-- ============================================================
CREATE TABLE dbo.[Phone] (
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Phone_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyId    UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    LabelId      TINYINT          NOT NULL, -- شناسه برچسب
    CountryId    SMALLINT         NOT NULL, -- شناسه کشور (phone number's country code context; separate from Company.RegistrationCountryId)
    PhoneNumber  VARCHAR(16)      NOT NULL, -- شماره تلفن (E.164: + و حداکثر ۱۵ رقم)
    IsPrimary    BIT              NOT NULL CONSTRAINT DF_Phone_IsPrimary DEFAULT 0, -- اصلی
    IsVerified   BIT              NOT NULL CONSTRAINT DF_Phone_IsVerified DEFAULT 0, -- تأیید شده
    VerifiedAt   DATETIME2(7)     NULL, -- تاریخ تأیید
    CreatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Phone_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Phone_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_Phone PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Phone_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Phone_Label FOREIGN KEY (LabelId) REFERENCES dbo.[PhoneLabel] (Id),
    CONSTRAINT UQ_Phone_CompanyNumber UNIQUE (CompanyId, CountryId, PhoneNumber),
    CONSTRAINT CK_Phone_VerifiedAt CHECK (IsVerified = 0 OR VerifiedAt IS NOT NULL),
    CONSTRAINT CK_Phone_E164 CHECK (
      PhoneNumber LIKE '+[0-9]%'
      AND CHARINDEX('+', PhoneNumber) = 1
      AND PhoneNumber NOT LIKE '%+%+%'
      AND LEN(PhoneNumber) BETWEEN 8 AND 16
      AND SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber)) NOT LIKE '%[^0-9]%'
      AND LEN(SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber))) BETWEEN 7 AND 15
    ) -- E.164: leading +, then 7–15 digits (total length 8–16)
);
CREATE NONCLUSTERED INDEX IX_Phone_CompanyId ON dbo.[Phone] (CompanyId);
CREATE NONCLUSTERED INDEX IX_Phone_LabelId ON dbo.[Phone] (LabelId);
CREATE NONCLUSTERED INDEX IX_Phone_CountryId ON dbo.[Phone] (CountryId);
CREATE NONCLUSTERED INDEX IX_Phone_Number ON dbo.[Phone] (PhoneNumber);
CREATE UNIQUE NONCLUSTERED INDEX UX_Phone_Company_Primary ON dbo.[Phone] (CompanyId) WHERE IsPrimary = 1;
GO

-- Address (depends on Company, AddressLabel)
-- ============================================================
CREATE TABLE dbo.[Address] (
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Address_Id DEFAULT NEWSEQUENTIALID(), -- شناسه
    CompanyId    UNIQUEIDENTIFIER NOT NULL, -- شناسه شرکت
    LabelId      TINYINT          NOT NULL, -- شناسه برچسب
    CountryId    SMALLINT         NOT NULL, -- شناسه کشور
    RegionId     SMALLINT         NOT NULL, -- شناسه استان
    CityId       INT              NOT NULL, -- شناسه شهر
    PostalCode   VARCHAR(20)      NOT NULL, -- کد پستی (چندکشوره؛ برخی بیش از ۱۰ کاراکتر یا حروف/فاصله)
    Latitude     DECIMAL(9, 6)    NULL, -- عرض جغرافیایی
    Longitude    DECIMAL(9, 6)    NULL, -- طول جغرافیایی
    IsPrimary    BIT              NOT NULL CONSTRAINT DF_Address_IsPrimary DEFAULT 0, -- اصلی
    IsVerified   BIT              NOT NULL CONSTRAINT DF_Address_IsVerified DEFAULT 0, -- تأیید شده
    VerifiedAt   DATETIME2(7)     NULL, -- تاریخ تأیید
    CreatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Address_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Address_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_Address PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Address_Company FOREIGN KEY (CompanyId) REFERENCES dbo.[Company] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Address_Label FOREIGN KEY (LabelId) REFERENCES dbo.[AddressLabel] (Id),
    CONSTRAINT CK_Address_VerifiedAt CHECK (IsVerified = 0 OR VerifiedAt IS NOT NULL)
);
CREATE NONCLUSTERED INDEX IX_Address_CompanyId ON dbo.[Address] (CompanyId);
CREATE NONCLUSTERED INDEX IX_Address_LabelId ON dbo.[Address] (LabelId);
CREATE NONCLUSTERED INDEX IX_Address_CountryId ON dbo.[Address] (CountryId);
CREATE NONCLUSTERED INDEX IX_Address_RegionId ON dbo.[Address] (RegionId);
CREATE NONCLUSTERED INDEX IX_Address_CityId ON dbo.[Address] (CityId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Address_Company_Primary ON dbo.[Address] (CompanyId) WHERE IsPrimary = 1;
GO

-- AddressTranslation (depends on Address)
-- ============================================================
CREATE TABLE dbo.[AddressTranslation] (
    AddressId  UNIQUEIDENTIFIER NOT NULL, -- شناسه آدرس
    LanguageId SMALLINT         NOT NULL, -- شناسه زبان
    Street1    NVARCHAR(100)    NOT NULL, -- خیابان ۱
    Street2    NVARCHAR(100)    NULL, -- خیابان ۲
    CONSTRAINT PK_AddressTranslation PRIMARY KEY CLUSTERED (AddressId, LanguageId),
    CONSTRAINT FK_AddressTranslation_Address FOREIGN KEY (AddressId) REFERENCES dbo.[Address] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_AddressTranslation_LanguageId ON dbo.[AddressTranslation] (LanguageId);
GO

-- ============================================================
-- UpdatedAt triggers (set UpdatedAt = SYSUTCDATETIME() on UPDATE; skip if UpdatedAt in UPDATE to reduce extra writes)
-- Note: IF UPDATE(UpdatedAt) returns true if UpdatedAt is in the SET clause, even if value unchanged.
-- This allows app-layer control: if an UPDATE includes UpdatedAt, the trigger skips (intentional design).
-- ============================================================
CREATE TRIGGER dbo.TR_Company_SetUpdatedAt ON dbo.[Company] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[Company] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_CompanyNameHistory_SetUpdatedAt ON dbo.[CompanyNameHistory] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[CompanyNameHistory] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_CompanyStakeholder_SetUpdatedAt ON dbo.[CompanyStakeholder] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[CompanyStakeholder] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_CompanyStakeholderHistory_SetUpdatedAt ON dbo.[CompanyStakeholderHistory] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[CompanyStakeholderHistory] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_Email_SetUpdatedAt ON dbo.[Email] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[Email] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_Phone_SetUpdatedAt ON dbo.[Phone] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[Phone] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO
CREATE TRIGGER dbo.TR_Address_SetUpdatedAt ON dbo.[Address] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[Address] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO

-- ============================================================
-- Overlap prevention triggers (prevent overlapping date ranges in history tables)
-- ============================================================
CREATE TRIGGER dbo.TR_CompanyNameHistory_PreventOverlap ON dbo.[CompanyNameHistory] AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  
  -- Check for overlaps within the inserted/updated rows themselves (multi-row operations)
  IF EXISTS (
    SELECT 1
    FROM inserted a
    INNER JOIN inserted b ON a.CompanyId = b.CompanyId AND a.Id < b.Id
    WHERE a.StartDate <= ISNULL(b.EndDate, '9999-12-31')
      AND ISNULL(a.EndDate, '9999-12-31') >= b.StartDate
  )
  BEGIN
    RAISERROR('Overlapping date ranges are not allowed for CompanyNameHistory. Each company can have only one active name history period at a time.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;
  
  -- Check for overlaps with existing rows
  IF EXISTS (
    SELECT 1
    FROM inserted i
    INNER JOIN dbo.[CompanyNameHistory] e ON e.CompanyId = i.CompanyId AND e.Id <> i.Id
    WHERE (
      (i.StartDate <= ISNULL(e.EndDate, '9999-12-31') AND ISNULL(i.EndDate, '9999-12-31') >= e.StartDate)
    )
  )
  BEGIN
    RAISERROR('Overlapping date ranges are not allowed for CompanyNameHistory. Each company can have only one active name history period at a time.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;
END;
GO
CREATE TRIGGER dbo.TR_CompanyStakeholderHistory_PreventOverlap ON dbo.[CompanyStakeholderHistory] AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  
  -- Check for overlaps within the inserted/updated rows themselves (multi-row operations)
  IF EXISTS (
    SELECT 1
    FROM inserted a
    INNER JOIN inserted b ON a.CompanyStakeholderId = b.CompanyStakeholderId AND a.Id < b.Id
    WHERE a.EffectiveDate <= ISNULL(b.EndDate, '9999-12-31')
      AND ISNULL(a.EndDate, '9999-12-31') >= b.EffectiveDate
  )
  BEGIN
    RAISERROR('Overlapping date ranges are not allowed for CompanyStakeholderHistory. Each stakeholder can have only one active history period at a time.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;
  
  -- Check for overlaps with existing rows
  IF EXISTS (
    SELECT 1
    FROM inserted i
    INNER JOIN dbo.[CompanyStakeholderHistory] e ON e.CompanyStakeholderId = i.CompanyStakeholderId AND e.Id <> i.Id
    WHERE (
      (i.EffectiveDate <= ISNULL(e.EndDate, '9999-12-31') AND ISNULL(i.EndDate, '9999-12-31') >= e.EffectiveDate)
    )
  )
  BEGIN
    RAISERROR('Overlapping date ranges are not allowed for CompanyStakeholderHistory. Each stakeholder can have only one active history period at a time.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END;
END;
GO

-- ============================================================
-- SECTION 6: Seed data (order matches table creation)
-- ============================================================

-- Seed: CompanyType
-- ============================================================
INSERT INTO dbo.[CompanyType] (Code) VALUES
    ('LLC'), ('Corporation'), ('Partnership'), ('SoleProprietorship'),
    ('Cooperative'), ('NonProfit'), ('Government'), ('PublicCompany'), ('Other');
GO

-- Seed CompanyTypeTranslation (en/fa). LanguageId from KSS_Common_Prod.dbo.[Language].
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('LLC',                 'en', N'Limited Liability Company'),
        ('LLC',                 'fa', N'شرکت با مسئولیت محدود'),
        ('Corporation',         'en', N'Corporation'),
        ('Corporation',         'fa', N'شرکت سهامی'),
        ('Partnership',         'en', N'Partnership'),
        ('Partnership',         'fa', N'شرکت تضامنی'),
        ('SoleProprietorship',  'en', N'Sole Proprietorship'),
        ('SoleProprietorship',  'fa', N'مالکیت انفرادی'),
        ('Cooperative',         'en', N'Cooperative'),
        ('Cooperative',         'fa', N'شرکت تعاونی'),
        ('NonProfit',           'en', N'Non-Profit Organization'),
        ('NonProfit',           'fa', N'سازمان غیرانتفاعی'),
        ('Government',          'en', N'Government Entity'),
        ('Government',          'fa', N'نهاد دولتی'),
        ('PublicCompany',       'en', N'Public Company'),
        ('PublicCompany',       'fa', N'شرکت سهامی عام'),
        ('Other',               'en', N'Other'),
        ('Other',               'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[CompanyTypeTranslation] (CompanyTypeId, LanguageId, Name)
SELECT ct.Id AS CompanyTypeId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[CompanyType] ct ON ct.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO

-- ============================================================
-- Seed: Industry
-- ============================================================
INSERT INTO dbo.[Industry] (Code) VALUES
    ('Technology'), ('Finance'), ('Healthcare'), ('Manufacturing'),
    ('Retail'), ('Construction'), ('Transportation'), ('Education'),
    ('Agriculture'), ('Energy'), ('Telecom'), ('RealEstate'),
    ('FoodBeverage'), ('Media'), ('Tourism'), ('Mining'), ('Other');
GO

-- Seed IndustryTranslation (en/fa).
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('Technology',      'en', N'Technology'),
        ('Technology',      'fa', N'فناوری'),
        ('Finance',         'en', N'Finance & Banking'),
        ('Finance',         'fa', N'مالی و بانکداری'),
        ('Healthcare',      'en', N'Healthcare'),
        ('Healthcare',      'fa', N'بهداشت و درمان'),
        ('Manufacturing',   'en', N'Manufacturing'),
        ('Manufacturing',   'fa', N'تولید و صنعت'),
        ('Retail',          'en', N'Retail & Wholesale'),
        ('Retail',          'fa', N'خرده‌فروشی و عمده‌فروشی'),
        ('Construction',    'en', N'Construction'),
        ('Construction',    'fa', N'ساخت و ساز'),
        ('Transportation',  'en', N'Transportation & Logistics'),
        ('Transportation',  'fa', N'حمل و نقل'),
        ('Education',       'en', N'Education'),
        ('Education',       'fa', N'آموزش'),
        ('Agriculture',     'en', N'Agriculture'),
        ('Agriculture',     'fa', N'کشاورزی'),
        ('Energy',          'en', N'Energy & Utilities'),
        ('Energy',          'fa', N'انرژی'),
        ('Telecom',         'en', N'Telecommunications'),
        ('Telecom',         'fa', N'مخابرات'),
        ('RealEstate',      'en', N'Real Estate'),
        ('RealEstate',      'fa', N'املاک و مستغلات'),
        ('FoodBeverage',    'en', N'Food & Beverage'),
        ('FoodBeverage',    'fa', N'مواد غذایی و نوشیدنی'),
        ('Media',           'en', N'Media & Entertainment'),
        ('Media',           'fa', N'رسانه و سرگرمی'),
        ('Tourism',         'en', N'Tourism & Hospitality'),
        ('Tourism',         'fa', N'گردشگری و هتلداری'),
        ('Mining',          'en', N'Mining & Minerals'),
        ('Mining',          'fa', N'معدن'),
        ('Other',           'en', N'Other'),
        ('Other',           'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[IndustryTranslation] (IndustryId, LanguageId, Name)
SELECT i.Id AS IndustryId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[Industry] i ON i.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO

-- ============================================================
-- Seed: EmailLabel
-- ============================================================
INSERT INTO dbo.[EmailLabel] (Code) VALUES
    ('General'), ('Support'), ('Sales'), ('HR'), ('Other');
GO
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('General', 'en', N'General'),   ('General', 'fa', N'عمومی'),
        ('Support', 'en', N'Support'),   ('Support', 'fa', N'پشتیبانی'),
        ('Sales',   'en', N'Sales'),     ('Sales',   'fa', N'فروش'),
        ('HR',      'en', N'HR'),        ('HR',      'fa', N'منابع انسانی'),
        ('Other',   'en', N'Other'),     ('Other',   'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[EmailLabelTranslation] (EmailLabelId, LanguageId, Name)
SELECT el.Id AS EmailLabelId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[EmailLabel] el ON el.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO

-- ============================================================
-- Seed: PhoneLabel
-- ============================================================
INSERT INTO dbo.[PhoneLabel] (Code) VALUES
    ('Office'), ('Fax'), ('Mobile'), ('Hotline'), ('Other');
GO
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('Office',  'en', N'Office'),    ('Office',  'fa', N'دفتر'),
        ('Fax',     'en', N'Fax'),       ('Fax',     'fa', N'فکس'),
        ('Mobile',  'en', N'Mobile'),    ('Mobile',  'fa', N'موبایل'),
        ('Hotline', 'en', N'Hotline'),   ('Hotline', 'fa', N'خط ویژه'),
        ('Other',   'en', N'Other'),     ('Other',   'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[PhoneLabelTranslation] (PhoneLabelId, LanguageId, Name)
SELECT pl.Id AS PhoneLabelId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[PhoneLabel] pl ON pl.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO

-- ============================================================
-- Seed: AddressLabel
-- ============================================================
INSERT INTO dbo.[AddressLabel] (Code) VALUES
    ('HeadOffice'), ('Branch'), ('Warehouse'), ('Factory'), ('Other');
GO
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('HeadOffice', 'en', N'Head Office'),  ('HeadOffice', 'fa', N'دفتر مرکزی'),
        ('Branch',     'en', N'Branch'),        ('Branch',     'fa', N'شعبه'),
        ('Warehouse',  'en', N'Warehouse'),     ('Warehouse',  'fa', N'انبار'),
        ('Factory',    'en', N'Factory'),        ('Factory',    'fa', N'کارخانه'),
        ('Other',      'en', N'Other'),          ('Other',      'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[AddressLabelTranslation] (AddressLabelId, LanguageId, Name)
SELECT al.Id AS AddressLabelId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[AddressLabel] al ON al.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO

-- ============================================================
-- Seed: StakeholderType
-- ============================================================
INSERT INTO dbo.[StakeholderType] (Code) VALUES
    ('Parent'), ('Subsidiary'), ('Investor'), ('Partner'), ('Shareholder'), ('Franchisor'), ('Franchisee'), ('Other');
GO
;WITH v AS (
    SELECT Code, LangCode, Name FROM (VALUES
        ('Parent',     'en', N'Parent Company'),   ('Parent',     'fa', N'شرکت مادر'),
        ('Subsidiary', 'en', N'Subsidiary'),       ('Subsidiary', 'fa', N'شرکت فرعی'),
        ('Investor',   'en', N'Investor'),         ('Investor',   'fa', N'سرمایه‌گذار'),
        ('Partner',    'en', N'Partner'),          ('Partner',    'fa', N'شریک'),
        ('Shareholder', 'en', N'Shareholder'),     ('Shareholder', 'fa', N'سهامدار'),
        ('Franchisor', 'en', N'Franchisor'),       ('Franchisor', 'fa', N'فرانچایزر'),
        ('Franchisee', 'en', N'Franchisee'),       ('Franchisee', 'fa', N'فرانچایزی'),
        ('Other',      'en', N'Other'),             ('Other',      'fa', N'سایر')
    ) AS x(Code, LangCode, Name)
)
INSERT INTO dbo.[StakeholderTypeTranslation] (StakeholderTypeId, LanguageId, Name)
SELECT st.Id AS StakeholderTypeId, l.Id AS LanguageId, v.Name
FROM v
JOIN dbo.[StakeholderType] st ON st.Code = v.Code
JOIN KSS_Common_Prod.dbo.[Language] l ON l.Code = v.LangCode;
GO
