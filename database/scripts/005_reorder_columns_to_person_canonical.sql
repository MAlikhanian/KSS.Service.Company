/* ============================================================================
 * 005_reorder_columns_to_person_canonical.sql
 * ----------------------------------------------------------------------------
 * Reorders columns on 8 tables in KSS_Company_* to match the Person service's
 * canonical column ordering:
 *   1. Id (PK) first
 *   2. Business / FK columns (NOT NULL first, then nullable)
 *   3. Boolean flags (IsPrimary, IsVerified, ...) if any
 *   4. Audit block at the end, in this exact order:
 *        CreatedBy   uniqueidentifier NOT NULL
 *        CreatedAt   datetime2        NOT NULL
 *        UpdatedBy   uniqueidentifier NULL
 *        UpdatedAt   datetime2        NULL   <-- changed from NOT NULL
 *        DeletedBy   uniqueidentifier NULL
 *        DeletedAt   datetime2        NULL
 *        IsActive    bit              NOT NULL DEFAULT 1
 *
 * Tables rebuilt (table-swap pattern, single transaction):
 *   Address, Company, Email, FinancialInfo, NameHistory, Phone,
 *   Stakeholder, StakeholderHistory
 *
 * Existing data preserved byte-for-byte (Id GUIDs, timestamps, etc.).
 *
 * Pre-reorder backup: D:\SQL Server\Backup\KSS_Company_Prod_PreReorder_20260513.bak
 * Rollback target if anything fails.
 * ========================================================================== */

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

/* ============================================================================
 * STEP 1 - Drop incoming FKs from the 3 translation/dependent tables
 * ========================================================================== */
ALTER TABLE dbo.AddressTranslation     DROP CONSTRAINT FK_AddressTranslation_Address;
ALTER TABLE dbo.Translation            DROP CONSTRAINT FK_Translation_Company;
ALTER TABLE dbo.NameHistoryTranslation DROP CONSTRAINT FK_NameHistoryTranslation_NameHistory;

/* ============================================================================
 * TABLE 1/8 - StakeholderHistory  (rebuild leaf table first)
 * ========================================================================== */
CREATE TABLE dbo.StakeholderHistory_New (
    Id                            uniqueidentifier NOT NULL CONSTRAINT DF_StakeholderHistory_Id_New          DEFAULT (NEWSEQUENTIALID()),
    CompanyStakeholderId          uniqueidentifier NOT NULL,
    OwnershipPercentage           decimal(5,2)     NOT NULL,
    ShareCount                    bigint           NOT NULL,
    RegistrationDate              date             NOT NULL,
    EffectiveDate                 date             NOT NULL,
    EndDate                       date             NULL,
    BoardRepresentativePersonId   uniqueidentifier NULL,
    CreatedBy                     uniqueidentifier NOT NULL,
    CreatedAt                     datetime2        NOT NULL,
    UpdatedBy                     uniqueidentifier NULL,
    UpdatedAt                     datetime2        NULL,
    DeletedBy                     uniqueidentifier NULL,
    DeletedAt                     datetime2        NULL,
    IsActive                      bit              NOT NULL CONSTRAINT DF_StakeholderHistory_IsActive_New    DEFAULT ((1))
);

INSERT INTO dbo.StakeholderHistory_New
    (Id, CompanyStakeholderId, OwnershipPercentage, ShareCount, RegistrationDate, EffectiveDate, EndDate, BoardRepresentativePersonId,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyStakeholderId, OwnershipPercentage, ShareCount, RegistrationDate, EffectiveDate, EndDate, BoardRepresentativePersonId,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.StakeholderHistory;

DROP TABLE dbo.StakeholderHistory;
EXEC sp_rename N'dbo.StakeholderHistory_New', N'StakeholderHistory';
EXEC sp_rename N'DF_StakeholderHistory_Id_New',       N'DF_StakeholderHistory_Id',       N'OBJECT';
EXEC sp_rename N'DF_StakeholderHistory_IsActive_New', N'DF_StakeholderHistory_IsActive', N'OBJECT';

ALTER TABLE dbo.StakeholderHistory ADD CONSTRAINT PK_StakeholderHistory PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_StakeholderHistory_BoardRepresentativePersonId ON dbo.StakeholderHistory (BoardRepresentativePersonId) WHERE ([BoardRepresentativePersonId] IS NOT NULL);
CREATE NONCLUSTERED INDEX IX_StakeholderHistory_EffectiveDate              ON dbo.StakeholderHistory (EffectiveDate);
CREATE NONCLUSTERED INDEX IX_StakeholderHistory_EndDate                    ON dbo.StakeholderHistory (EndDate) WHERE ([EndDate] IS NOT NULL);
CREATE NONCLUSTERED INDEX IX_StakeholderHistory_RegistrationDate           ON dbo.StakeholderHistory (RegistrationDate);
CREATE NONCLUSTERED INDEX IX_StakeholderHistory_StakeholderId              ON dbo.StakeholderHistory (CompanyStakeholderId);
CREATE UNIQUE NONCLUSTERED INDEX UX_StakeholderHistory_Current ON dbo.StakeholderHistory (CompanyStakeholderId) WHERE ([EndDate] IS NULL);
CREATE UNIQUE NONCLUSTERED INDEX UX_StakeholderHistory_Stakeholder_EffectiveDate ON dbo.StakeholderHistory (CompanyStakeholderId, EffectiveDate);

/* ============================================================================
 * TABLE 2/8 - Stakeholder
 * ========================================================================== */
CREATE TABLE dbo.Stakeholder_New (
    Id                  uniqueidentifier NOT NULL CONSTRAINT DF_Stakeholder_Id_New       DEFAULT (NEWSEQUENTIALID()),
    CompanyId           uniqueidentifier NOT NULL,
    StakeholderTypeId   tinyint          NOT NULL,
    RelatedPartyType    tinyint          NOT NULL,
    RelatedPartyId      uniqueidentifier NOT NULL,
    CreatedBy           uniqueidentifier NOT NULL,
    CreatedAt           datetime2        NOT NULL,
    UpdatedBy           uniqueidentifier NULL,
    UpdatedAt           datetime2        NULL,
    DeletedBy           uniqueidentifier NULL,
    DeletedAt           datetime2        NULL,
    IsActive            bit              NOT NULL CONSTRAINT DF_Stakeholder_IsActive_New DEFAULT ((1))
);

INSERT INTO dbo.Stakeholder_New
    (Id, CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.Stakeholder;

-- StakeholderHistory's FK_StakeholderHistory_Stakeholder doesn't exist yet
-- (we just rebuilt StakeholderHistory above without it), so dropping
-- Stakeholder won't be blocked.
DROP TABLE dbo.Stakeholder;
EXEC sp_rename N'dbo.Stakeholder_New', N'Stakeholder';
EXEC sp_rename N'DF_Stakeholder_Id_New',       N'DF_Stakeholder_Id',       N'OBJECT';
EXEC sp_rename N'DF_Stakeholder_IsActive_New', N'DF_Stakeholder_IsActive', N'OBJECT';

ALTER TABLE dbo.Stakeholder ADD CONSTRAINT PK_Stakeholder PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_Stakeholder_CompanyId          ON dbo.Stakeholder (CompanyId);
CREATE NONCLUSTERED INDEX IX_Stakeholder_RelatedParty       ON dbo.Stakeholder (RelatedPartyType, RelatedPartyId);
CREATE NONCLUSTERED INDEX IX_Stakeholder_StakeholderTypeId  ON dbo.Stakeholder (StakeholderTypeId);
ALTER TABLE dbo.Stakeholder ADD CONSTRAINT UQ_Stakeholder UNIQUE (CompanyId, StakeholderTypeId, RelatedPartyType, RelatedPartyId);

/* ============================================================================
 * TABLE 3/8 - FinancialInfo
 * ========================================================================== */
CREATE TABLE dbo.FinancialInfo_New (
    Id                 uniqueidentifier NOT NULL CONSTRAINT DF_FinancialInfo_Id_New                  DEFAULT (NEWSEQUENTIALID()),
    CompanyId          uniqueidentifier NOT NULL,
    FiscalYear         smallint         NOT NULL,
    RegisteredCapital  decimal(18,0)    NOT NULL CONSTRAINT DF_FinancialInfo_RegisteredCapital_New   DEFAULT ((0)),
    NumberOfShares     bigint           NOT NULL CONSTRAINT DF_FinancialInfo_NumberOfShares_New      DEFAULT ((0)),
    CreatedBy          uniqueidentifier NOT NULL,
    CreatedAt          datetime2        NOT NULL,
    UpdatedBy          uniqueidentifier NULL,
    UpdatedAt          datetime2        NULL,
    DeletedBy          uniqueidentifier NULL,
    DeletedAt          datetime2        NULL,
    IsActive           bit              NOT NULL CONSTRAINT DF_FinancialInfo_IsActive_New            DEFAULT ((1))
);

INSERT INTO dbo.FinancialInfo_New
    (Id, CompanyId, FiscalYear, RegisteredCapital, NumberOfShares,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, FiscalYear, RegisteredCapital, NumberOfShares,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.FinancialInfo;

DROP TABLE dbo.FinancialInfo;
EXEC sp_rename N'dbo.FinancialInfo_New', N'FinancialInfo';
EXEC sp_rename N'DF_FinancialInfo_Id_New',                N'DF_FinancialInfo_Id',                N'OBJECT';
EXEC sp_rename N'DF_FinancialInfo_RegisteredCapital_New', N'DF_FinancialInfo_RegisteredCapital', N'OBJECT';
EXEC sp_rename N'DF_FinancialInfo_NumberOfShares_New',    N'DF_FinancialInfo_NumberOfShares',    N'OBJECT';
EXEC sp_rename N'DF_FinancialInfo_IsActive_New',          N'DF_FinancialInfo_IsActive',          N'OBJECT';

ALTER TABLE dbo.FinancialInfo ADD CONSTRAINT PK_FinancialInfo PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_FinancialInfo_CompanyId   ON dbo.FinancialInfo (CompanyId);
CREATE NONCLUSTERED INDEX IX_FinancialInfo_FiscalYear  ON dbo.FinancialInfo (FiscalYear);
ALTER TABLE dbo.FinancialInfo ADD CONSTRAINT UX_FinancialInfo_Company_Year UNIQUE (CompanyId, FiscalYear);

/* ============================================================================
 * TABLE 4/8 - NameHistory
 *   Description column kept after EndDate but before audit block.
 * ========================================================================== */
CREATE TABLE dbo.NameHistory_New (
    Id           uniqueidentifier NOT NULL CONSTRAINT DF_NameHistory_Id_New       DEFAULT (NEWSEQUENTIALID()),
    CompanyId    uniqueidentifier NOT NULL,
    StartDate    date             NOT NULL,
    EndDate      date             NULL,
    Description  nvarchar(500)    COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CreatedBy    uniqueidentifier NOT NULL,
    CreatedAt    datetime2        NOT NULL,
    UpdatedBy    uniqueidentifier NULL,
    UpdatedAt    datetime2        NULL,
    DeletedBy    uniqueidentifier NULL,
    DeletedAt    datetime2        NULL,
    IsActive     bit              NOT NULL CONSTRAINT DF_NameHistory_IsActive_New DEFAULT ((1))
);

INSERT INTO dbo.NameHistory_New
    (Id, CompanyId, StartDate, EndDate, Description,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, StartDate, EndDate, Description,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.NameHistory;

DROP TABLE dbo.NameHistory;
EXEC sp_rename N'dbo.NameHistory_New', N'NameHistory';
EXEC sp_rename N'DF_NameHistory_Id_New',       N'DF_NameHistory_Id',       N'OBJECT';
EXEC sp_rename N'DF_NameHistory_IsActive_New', N'DF_NameHistory_IsActive', N'OBJECT';

ALTER TABLE dbo.NameHistory ADD CONSTRAINT PK_NameHistory PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_NameHistory_CompanyId ON dbo.NameHistory (CompanyId);
CREATE NONCLUSTERED INDEX IX_NameHistory_EndDate   ON dbo.NameHistory (EndDate) WHERE ([EndDate] IS NOT NULL);
CREATE NONCLUSTERED INDEX IX_NameHistory_StartDate ON dbo.NameHistory (StartDate);
CREATE UNIQUE NONCLUSTERED INDEX UX_NameHistory_Current ON dbo.NameHistory (CompanyId) WHERE ([EndDate] IS NULL);

/* ============================================================================
 * TABLE 5/8 - Phone
 * ========================================================================== */
CREATE TABLE dbo.Phone_New (
    Id          uniqueidentifier NOT NULL CONSTRAINT DF_Phone_Id_New         DEFAULT (NEWSEQUENTIALID()),
    CompanyId   uniqueidentifier NOT NULL,
    LabelId     tinyint          NOT NULL,
    CountryId   smallint         NOT NULL,
    PhoneNumber varchar(16)      COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    VerifiedAt  datetime2        NULL,
    IsPrimary   bit              NOT NULL CONSTRAINT DF_Phone_IsPrimary_New  DEFAULT ((0)),
    IsVerified  bit              NOT NULL CONSTRAINT DF_Phone_IsVerified_New DEFAULT ((0)),
    CreatedBy   uniqueidentifier NOT NULL,
    CreatedAt   datetime2        NOT NULL,
    UpdatedBy   uniqueidentifier NULL,
    UpdatedAt   datetime2        NULL,
    DeletedBy   uniqueidentifier NULL,
    DeletedAt   datetime2        NULL,
    IsActive    bit              NOT NULL CONSTRAINT DF_Phone_IsActive_New   DEFAULT ((1))
);

INSERT INTO dbo.Phone_New
    (Id, CompanyId, LabelId, CountryId, PhoneNumber, VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, LabelId, CountryId, PhoneNumber, VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.Phone;

DROP TABLE dbo.Phone;
EXEC sp_rename N'dbo.Phone_New', N'Phone';
EXEC sp_rename N'DF_Phone_Id_New',         N'DF_Phone_Id',         N'OBJECT';
EXEC sp_rename N'DF_Phone_IsPrimary_New',  N'DF_Phone_IsPrimary',  N'OBJECT';
EXEC sp_rename N'DF_Phone_IsVerified_New', N'DF_Phone_IsVerified', N'OBJECT';
EXEC sp_rename N'DF_Phone_IsActive_New',   N'DF_Phone_IsActive',   N'OBJECT';

ALTER TABLE dbo.Phone ADD CONSTRAINT PK_Phone PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_Phone_CompanyId ON dbo.Phone (CompanyId);
CREATE NONCLUSTERED INDEX IX_Phone_CountryId ON dbo.Phone (CountryId);
CREATE NONCLUSTERED INDEX IX_Phone_LabelId   ON dbo.Phone (LabelId);
CREATE NONCLUSTERED INDEX IX_Phone_Number    ON dbo.Phone (PhoneNumber);
ALTER TABLE dbo.Phone ADD CONSTRAINT UQ_Phone_CompanyNumber UNIQUE (CompanyId, CountryId, PhoneNumber);
CREATE UNIQUE NONCLUSTERED INDEX UX_Phone_Company_Primary ON dbo.Phone (CompanyId) WHERE ([IsPrimary]=(1));

/* ============================================================================
 * TABLE 6/8 - Email
 * ========================================================================== */
CREATE TABLE dbo.Email_New (
    Id          uniqueidentifier NOT NULL CONSTRAINT DF_Email_Id_New         DEFAULT (NEWSEQUENTIALID()),
    CompanyId   uniqueidentifier NOT NULL,
    LabelId     tinyint          NOT NULL,
    [Email]     varchar(128)     COLLATE Latin1_General_100_CI_AS NOT NULL,
    VerifiedAt  datetime2        NULL,
    IsPrimary   bit              NOT NULL CONSTRAINT DF_Email_IsPrimary_New  DEFAULT ((0)),
    IsVerified  bit              NOT NULL CONSTRAINT DF_Email_IsVerified_New DEFAULT ((0)),
    CreatedBy   uniqueidentifier NOT NULL,
    CreatedAt   datetime2        NOT NULL,
    UpdatedBy   uniqueidentifier NULL,
    UpdatedAt   datetime2        NULL,
    DeletedBy   uniqueidentifier NULL,
    DeletedAt   datetime2        NULL,
    IsActive    bit              NOT NULL CONSTRAINT DF_Email_IsActive_New   DEFAULT ((1))
);

INSERT INTO dbo.Email_New
    (Id, CompanyId, LabelId, [Email], VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, LabelId, [Email], VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.Email;

DROP TABLE dbo.Email;
EXEC sp_rename N'dbo.Email_New', N'Email';
EXEC sp_rename N'DF_Email_Id_New',         N'DF_Email_Id',         N'OBJECT';
EXEC sp_rename N'DF_Email_IsPrimary_New',  N'DF_Email_IsPrimary',  N'OBJECT';
EXEC sp_rename N'DF_Email_IsVerified_New', N'DF_Email_IsVerified', N'OBJECT';
EXEC sp_rename N'DF_Email_IsActive_New',   N'DF_Email_IsActive',   N'OBJECT';

ALTER TABLE dbo.Email ADD CONSTRAINT PK_Email PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_Email_CompanyId ON dbo.Email (CompanyId);
CREATE NONCLUSTERED INDEX IX_Email_Email     ON dbo.Email ([Email]);
CREATE NONCLUSTERED INDEX IX_Email_LabelId   ON dbo.Email (LabelId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Email_Company_Email   ON dbo.Email (CompanyId, [Email]);
CREATE UNIQUE NONCLUSTERED INDEX UX_Email_Company_Primary ON dbo.Email (CompanyId) WHERE ([IsPrimary]=(1));

/* ============================================================================
 * TABLE 7/8 - Address
 * ========================================================================== */
CREATE TABLE dbo.Address_New (
    Id          uniqueidentifier NOT NULL CONSTRAINT DF_Address_Id_New         DEFAULT (NEWSEQUENTIALID()),
    CompanyId   uniqueidentifier NOT NULL,
    LabelId     tinyint          NOT NULL,
    CountryId   smallint         NOT NULL,
    RegionId    smallint         NOT NULL,
    CityId      int              NOT NULL,
    PostalCode  varchar(20)      COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Latitude    decimal(9,6)     NULL,
    Longitude   decimal(9,6)     NULL,
    VerifiedAt  datetime2        NULL,
    IsPrimary   bit              NOT NULL CONSTRAINT DF_Address_IsPrimary_New  DEFAULT ((0)),
    IsVerified  bit              NOT NULL CONSTRAINT DF_Address_IsVerified_New DEFAULT ((0)),
    CreatedBy   uniqueidentifier NOT NULL,
    CreatedAt   datetime2        NOT NULL,
    UpdatedBy   uniqueidentifier NULL,
    UpdatedAt   datetime2        NULL,
    DeletedBy   uniqueidentifier NULL,
    DeletedAt   datetime2        NULL,
    IsActive    bit              NOT NULL CONSTRAINT DF_Address_IsActive_New   DEFAULT ((1))
);

INSERT INTO dbo.Address_New
    (Id, CompanyId, LabelId, CountryId, RegionId, CityId, PostalCode, Latitude, Longitude, VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, CompanyId, LabelId, CountryId, RegionId, CityId, PostalCode, Latitude, Longitude, VerifiedAt, IsPrimary, IsVerified,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.Address;

DROP TABLE dbo.Address;
EXEC sp_rename N'dbo.Address_New', N'Address';
EXEC sp_rename N'DF_Address_Id_New',         N'DF_Address_Id',         N'OBJECT';
EXEC sp_rename N'DF_Address_IsPrimary_New',  N'DF_Address_IsPrimary',  N'OBJECT';
EXEC sp_rename N'DF_Address_IsVerified_New', N'DF_Address_IsVerified', N'OBJECT';
EXEC sp_rename N'DF_Address_IsActive_New',   N'DF_Address_IsActive',   N'OBJECT';

ALTER TABLE dbo.Address ADD CONSTRAINT PK_Address PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_Address_CityId    ON dbo.Address (CityId);
CREATE NONCLUSTERED INDEX IX_Address_CompanyId ON dbo.Address (CompanyId);
CREATE NONCLUSTERED INDEX IX_Address_CountryId ON dbo.Address (CountryId);
CREATE NONCLUSTERED INDEX IX_Address_LabelId   ON dbo.Address (LabelId);
CREATE NONCLUSTERED INDEX IX_Address_RegionId  ON dbo.Address (RegionId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Address_Company_Primary ON dbo.Address (CompanyId) WHERE ([IsPrimary]=(1));

/* ============================================================================
 * TABLE 8/8 - Company  (root, rebuilt last)
 * ========================================================================== */
CREATE TABLE dbo.Company_New (
    Id                     uniqueidentifier NOT NULL CONSTRAINT DF_Company_Id_New       DEFAULT (NEWSEQUENTIALID()),
    LegalFormId            tinyint          NOT NULL,
    RegistrationDate       date             NOT NULL,
    RegistrationNo         varchar(30)      COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NationalId             varchar(20)      COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    EconomicCode           varchar(20)      COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    RegistrationCountryId  smallint         NOT NULL,
    RegistrationRegionId   smallint         NOT NULL,
    RegistrationCityId     int              NOT NULL,
    IndustryId             smallint         NULL,
    TaxId                  varchar(30)      COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    FoundedDate            date             NULL,
    Website                varchar(256)     COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    LogoUrl                varchar(512)     COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CreatedBy              uniqueidentifier NOT NULL,
    CreatedAt              datetime2        NOT NULL,
    UpdatedBy              uniqueidentifier NULL,
    UpdatedAt              datetime2        NULL,
    DeletedBy              uniqueidentifier NULL,
    DeletedAt              datetime2        NULL,
    IsActive               bit              NOT NULL CONSTRAINT DF_Company_IsActive_New DEFAULT ((1))
);

INSERT INTO dbo.Company_New
    (Id, LegalFormId, RegistrationDate, RegistrationNo, NationalId, EconomicCode,
     RegistrationCountryId, RegistrationRegionId, RegistrationCityId,
     IndustryId, TaxId, FoundedDate, Website, LogoUrl,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive)
SELECT
     Id, LegalFormId, RegistrationDate, RegistrationNo, NationalId, EconomicCode,
     RegistrationCountryId, RegistrationRegionId, RegistrationCityId,
     IndustryId, TaxId, FoundedDate, Website, LogoUrl,
     CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt, IsActive
FROM dbo.Company;

-- At this point the outgoing FKs from Address/Email/Phone/FinancialInfo/
-- NameHistory/Stakeholder to Company haven't been recreated yet (the rebuilt
-- tables above don't have them yet), so DROP TABLE dbo.Company is unblocked.
DROP TABLE dbo.Company;
EXEC sp_rename N'dbo.Company_New', N'Company';
EXEC sp_rename N'DF_Company_Id_New',       N'DF_Company_Id',       N'OBJECT';
EXEC sp_rename N'DF_Company_IsActive_New', N'DF_Company_IsActive', N'OBJECT';

ALTER TABLE dbo.Company ADD CONSTRAINT PK_Company PRIMARY KEY CLUSTERED (Id);

CREATE NONCLUSTERED INDEX IX_Company_IndustryId             ON dbo.Company (IndustryId) WHERE ([IndustryId] IS NOT NULL);
CREATE NONCLUSTERED INDEX IX_Company_IsActive               ON dbo.Company (IsActive)   WHERE ([IsActive]=(1));
CREATE NONCLUSTERED INDEX IX_Company_LegalFormId            ON dbo.Company (LegalFormId);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationCityId     ON dbo.Company (RegistrationCityId);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationCountryId  ON dbo.Company (RegistrationCountryId);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationDate       ON dbo.Company (RegistrationDate);
CREATE NONCLUSTERED INDEX IX_Company_RegistrationRegionId   ON dbo.Company (RegistrationRegionId);
CREATE NONCLUSTERED INDEX IX_Company_TaxId                  ON dbo.Company (TaxId) WHERE ([TaxId] IS NOT NULL);
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_EconomicCode_Country   ON dbo.Company (RegistrationCountryId, EconomicCode);
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_NationalId_Country     ON dbo.Company (RegistrationCountryId, NationalId);
CREATE UNIQUE NONCLUSTERED INDEX UX_Company_RegistrationNo_Country ON dbo.Company (RegistrationCountryId, RegistrationNo);

/* ============================================================================
 * Recreate outgoing FKs (all 13) now that every table exists
 * ========================================================================== */
ALTER TABLE dbo.Address            ADD CONSTRAINT FK_Address_Company             FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.Address            ADD CONSTRAINT FK_Address_Label               FOREIGN KEY (LabelId)              REFERENCES dbo.AddressLabel(Id);
ALTER TABLE dbo.Company            ADD CONSTRAINT FK_Company_Industry            FOREIGN KEY (IndustryId)           REFERENCES dbo.Industry(Id);
ALTER TABLE dbo.Company            ADD CONSTRAINT FK_Company_LegalForm           FOREIGN KEY (LegalFormId)          REFERENCES dbo.LegalForm(Id);
ALTER TABLE dbo.Email              ADD CONSTRAINT FK_Email_Company               FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.Email              ADD CONSTRAINT FK_Email_Label                 FOREIGN KEY (LabelId)              REFERENCES dbo.EmailLabel(Id);
ALTER TABLE dbo.FinancialInfo      ADD CONSTRAINT FK_FinancialInfo_Company       FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.NameHistory        ADD CONSTRAINT FK_NameHistory_Company         FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.Phone              ADD CONSTRAINT FK_Phone_Company               FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.Phone              ADD CONSTRAINT FK_Phone_Label                 FOREIGN KEY (LabelId)              REFERENCES dbo.PhoneLabel(Id);
ALTER TABLE dbo.Stakeholder        ADD CONSTRAINT FK_Stakeholder_Company         FOREIGN KEY (CompanyId)            REFERENCES dbo.Company(Id)         ON DELETE CASCADE;
ALTER TABLE dbo.Stakeholder        ADD CONSTRAINT FK_Stakeholder_StakeholderType FOREIGN KEY (StakeholderTypeId)    REFERENCES dbo.StakeholderType(Id);
ALTER TABLE dbo.StakeholderHistory ADD CONSTRAINT FK_StakeholderHistory_Stakeholder FOREIGN KEY (CompanyStakeholderId) REFERENCES dbo.Stakeholder(Id) ON DELETE CASCADE;

/* ============================================================================
 * Recreate the 3 incoming FKs from the translation tables
 * ========================================================================== */
ALTER TABLE dbo.AddressTranslation     ADD CONSTRAINT FK_AddressTranslation_Address           FOREIGN KEY (AddressId)     REFERENCES dbo.Address(Id)     ON DELETE CASCADE;
ALTER TABLE dbo.Translation            ADD CONSTRAINT FK_Translation_Company                  FOREIGN KEY (CompanyId)     REFERENCES dbo.Company(Id)     ON DELETE CASCADE;
ALTER TABLE dbo.NameHistoryTranslation ADD CONSTRAINT FK_NameHistoryTranslation_NameHistory   FOREIGN KEY (NameHistoryId) REFERENCES dbo.NameHistory(Id) ON DELETE CASCADE;

COMMIT TRANSACTION;
GO

/* ============================================================================
 * Recreate the 8 SetUpdatedAt triggers (outside transaction, must be batch)
 *   - 4 keep their existing clean names (Address, Company, Email, Phone)
 *   - 4 are renamed to the clean form (FinancialInfo, NameHistory,
 *     Stakeholder, StakeholderHistory) — the old prefixed names die
 *     with the dropped tables.
 * ========================================================================== */
GO
CREATE TRIGGER dbo.TR_Address_SetUpdatedAt ON dbo.[Address] AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.[Address] c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_Company_SetUpdatedAt ON dbo.Company AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.Company c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_Email_SetUpdatedAt ON dbo.Email AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.Email c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_Phone_SetUpdatedAt ON dbo.Phone AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.Phone c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_FinancialInfo_SetUpdatedAt ON dbo.FinancialInfo AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.FinancialInfo c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_NameHistory_SetUpdatedAt ON dbo.NameHistory AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.NameHistory c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_Stakeholder_SetUpdatedAt ON dbo.Stakeholder AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.Stakeholder c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
CREATE TRIGGER dbo.TR_StakeholderHistory_SetUpdatedAt ON dbo.StakeholderHistory AFTER UPDATE
AS BEGIN SET NOCOUNT ON; IF UPDATE(UpdatedAt) RETURN; UPDATE c SET UpdatedAt = SYSUTCDATETIME() FROM dbo.StakeholderHistory c INNER JOIN inserted i ON i.Id = c.Id; END;
GO
