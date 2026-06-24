-- 004_add_audit_columns.sql
--
-- Brings audit-column coverage on every Company DB table in line with the
-- Person service pattern (the canonical KSS template).
--
-- Audit block (in conceptual Person order — but appended to the end of
-- existing tables, so the physical column order won't exactly match Person
-- for tables that already had CreatedAt/UpdatedAt/IsActive. Fixing physical
-- order requires drop+recreate of each table — skipped for this pass.):
--   CreatedBy  UNIQUEIDENTIFIER NOT NULL    (default '...000001' for backfill)
--   CreatedAt  DATETIME2        NOT NULL    (default SYSUTCDATETIME() for backfill)
--   UpdatedBy  UNIQUEIDENTIFIER NULL
--   UpdatedAt  DATETIME2        NULL
--   DeletedBy  UNIQUEIDENTIFIER NULL
--   DeletedAt  DATETIME2        NULL
--   IsActive   BIT              NOT NULL DEFAULT 1   (entity/lookup tables only;
--                                                    Translation tables skip it)
--
-- Tables already complete (skipped):
--   Access, AccessSection, RoleAccess      — 7/7 audit columns
--   AccessSectionTranslation               — 6/6 (translation, no IsActive)
--
-- Apply to KSS_Company_Prod and KSS_Company_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

BEGIN TRANSACTION;

-- ────────────────────────────────────────────────────────────────────────
-- GROUP 1 — Entity/lookup tables with NO audit columns: add all 7
-- (CreatedBy/At, UpdatedBy/At, DeletedBy/At, IsActive)
-- ────────────────────────────────────────────────────────────────────────

-- Industry
ALTER TABLE dbo.Industry ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Industry_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Industry_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_Industry_IsActive DEFAULT 1;
ALTER TABLE dbo.Industry DROP CONSTRAINT DF_Industry_CreatedBy;
ALTER TABLE dbo.Industry DROP CONSTRAINT DF_Industry_CreatedAt;

-- LegalForm
ALTER TABLE dbo.LegalForm ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_LegalForm_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_LegalForm_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_LegalForm_IsActive DEFAULT 1;
ALTER TABLE dbo.LegalForm DROP CONSTRAINT DF_LegalForm_CreatedBy;
ALTER TABLE dbo.LegalForm DROP CONSTRAINT DF_LegalForm_CreatedAt;

-- StakeholderType
ALTER TABLE dbo.StakeholderType ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_StakeholderType_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StakeholderType_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_StakeholderType_IsActive DEFAULT 1;
ALTER TABLE dbo.StakeholderType DROP CONSTRAINT DF_StakeholderType_CreatedBy;
ALTER TABLE dbo.StakeholderType DROP CONSTRAINT DF_StakeholderType_CreatedAt;

-- AddressLabel
ALTER TABLE dbo.AddressLabel ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AddressLabel_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AddressLabel_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_AddressLabel_IsActive DEFAULT 1;
ALTER TABLE dbo.AddressLabel DROP CONSTRAINT DF_AddressLabel_CreatedBy;
ALTER TABLE dbo.AddressLabel DROP CONSTRAINT DF_AddressLabel_CreatedAt;

-- EmailLabel
ALTER TABLE dbo.EmailLabel ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmailLabel_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_EmailLabel_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_EmailLabel_IsActive DEFAULT 1;
ALTER TABLE dbo.EmailLabel DROP CONSTRAINT DF_EmailLabel_CreatedBy;
ALTER TABLE dbo.EmailLabel DROP CONSTRAINT DF_EmailLabel_CreatedAt;

-- PhoneLabel
ALTER TABLE dbo.PhoneLabel ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PhoneLabel_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PhoneLabel_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_PhoneLabel_IsActive DEFAULT 1;
ALTER TABLE dbo.PhoneLabel DROP CONSTRAINT DF_PhoneLabel_CreatedBy;
ALTER TABLE dbo.PhoneLabel DROP CONSTRAINT DF_PhoneLabel_CreatedAt;

-- ────────────────────────────────────────────────────────────────────────
-- GROUP 2 — Translation tables with NO audit columns: add 6
-- (no IsActive — translations are never "deactivated", they get deleted)
-- ────────────────────────────────────────────────────────────────────────

-- Translation (was CompanyTranslation)
ALTER TABLE dbo.Translation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Translation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Translation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.Translation DROP CONSTRAINT DF_Translation_CreatedBy;
ALTER TABLE dbo.Translation DROP CONSTRAINT DF_Translation_CreatedAt;

-- NameHistoryTranslation
ALTER TABLE dbo.NameHistoryTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NameHistoryTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NameHistoryTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.NameHistoryTranslation DROP CONSTRAINT DF_NameHistoryTranslation_CreatedBy;
ALTER TABLE dbo.NameHistoryTranslation DROP CONSTRAINT DF_NameHistoryTranslation_CreatedAt;

-- IndustryTranslation
ALTER TABLE dbo.IndustryTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndustryTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_IndustryTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.IndustryTranslation DROP CONSTRAINT DF_IndustryTranslation_CreatedBy;
ALTER TABLE dbo.IndustryTranslation DROP CONSTRAINT DF_IndustryTranslation_CreatedAt;

-- LegalFormTranslation
ALTER TABLE dbo.LegalFormTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_LegalFormTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_LegalFormTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.LegalFormTranslation DROP CONSTRAINT DF_LegalFormTranslation_CreatedBy;
ALTER TABLE dbo.LegalFormTranslation DROP CONSTRAINT DF_LegalFormTranslation_CreatedAt;

-- StakeholderTypeTranslation
ALTER TABLE dbo.StakeholderTypeTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_StakeholderTypeTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StakeholderTypeTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.StakeholderTypeTranslation DROP CONSTRAINT DF_StakeholderTypeTranslation_CreatedBy;
ALTER TABLE dbo.StakeholderTypeTranslation DROP CONSTRAINT DF_StakeholderTypeTranslation_CreatedAt;

-- AddressLabelTranslation
ALTER TABLE dbo.AddressLabelTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AddressLabelTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AddressLabelTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.AddressLabelTranslation DROP CONSTRAINT DF_AddressLabelTranslation_CreatedBy;
ALTER TABLE dbo.AddressLabelTranslation DROP CONSTRAINT DF_AddressLabelTranslation_CreatedAt;

-- EmailLabelTranslation
ALTER TABLE dbo.EmailLabelTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmailLabelTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_EmailLabelTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.EmailLabelTranslation DROP CONSTRAINT DF_EmailLabelTranslation_CreatedBy;
ALTER TABLE dbo.EmailLabelTranslation DROP CONSTRAINT DF_EmailLabelTranslation_CreatedAt;

-- PhoneLabelTranslation
ALTER TABLE dbo.PhoneLabelTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PhoneLabelTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PhoneLabelTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.PhoneLabelTranslation DROP CONSTRAINT DF_PhoneLabelTranslation_CreatedBy;
ALTER TABLE dbo.PhoneLabelTranslation DROP CONSTRAINT DF_PhoneLabelTranslation_CreatedAt;

-- AddressTranslation
ALTER TABLE dbo.AddressTranslation ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AddressTranslation_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AddressTranslation_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.AddressTranslation DROP CONSTRAINT DF_AddressTranslation_CreatedBy;
ALTER TABLE dbo.AddressTranslation DROP CONSTRAINT DF_AddressTranslation_CreatedAt;

-- ────────────────────────────────────────────────────────────────────────
-- GROUP 3 — Tables that already have CreatedAt+UpdatedAt:
-- add the missing 5 (CreatedBy, UpdatedBy, DeletedBy, DeletedAt, IsActive)
-- ────────────────────────────────────────────────────────────────────────

-- Address (already has CreatedAt, UpdatedAt)
ALTER TABLE dbo.Address ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Address_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_Address_IsActive DEFAULT 1;
ALTER TABLE dbo.Address DROP CONSTRAINT DF_Address_CreatedBy;

-- Email (already has CreatedAt, UpdatedAt)
ALTER TABLE dbo.Email ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Email_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_Email_IsActive DEFAULT 1;
ALTER TABLE dbo.Email DROP CONSTRAINT DF_Email_CreatedBy;

-- Phone (already has CreatedAt, UpdatedAt)
ALTER TABLE dbo.Phone ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Phone_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_Phone_IsActive DEFAULT 1;
ALTER TABLE dbo.Phone DROP CONSTRAINT DF_Phone_CreatedBy;

-- FinancialInfo (was CompanyFinancialInfo)
ALTER TABLE dbo.FinancialInfo ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FinancialInfo_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_FinancialInfo_IsActive DEFAULT 1;
ALTER TABLE dbo.FinancialInfo DROP CONSTRAINT DF_FinancialInfo_CreatedBy;

-- NameHistory (was CompanyNameHistory)
ALTER TABLE dbo.NameHistory ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NameHistory_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_NameHistory_IsActive DEFAULT 1;
ALTER TABLE dbo.NameHistory DROP CONSTRAINT DF_NameHistory_CreatedBy;

-- Stakeholder (was CompanyStakeholder)
ALTER TABLE dbo.Stakeholder ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Stakeholder_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_Stakeholder_IsActive DEFAULT 1;
ALTER TABLE dbo.Stakeholder DROP CONSTRAINT DF_Stakeholder_CreatedBy;

-- StakeholderHistory (was CompanyStakeholderHistory)
ALTER TABLE dbo.StakeholderHistory ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_StakeholderHistory_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive  BIT NOT NULL CONSTRAINT DF_StakeholderHistory_IsActive DEFAULT 1;
ALTER TABLE dbo.StakeholderHistory DROP CONSTRAINT DF_StakeholderHistory_CreatedBy;

-- ────────────────────────────────────────────────────────────────────────
-- GROUP 4 — Company root: already has CreatedAt + UpdatedAt + IsActive.
-- Add the missing 4 (CreatedBy, UpdatedBy, DeletedBy, DeletedAt).
-- ────────────────────────────────────────────────────────────────────────

ALTER TABLE dbo.Company ADD
    CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Company_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000001',
    UpdatedBy UNIQUEIDENTIFIER NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL;
ALTER TABLE dbo.Company DROP CONSTRAINT DF_Company_CreatedBy;

COMMIT TRANSACTION;

-- ────────────────────────────────────────────────────────────────────────
-- Verification — every table now has CreatedBy + CreatedAt; entities also
-- have IsActive; translations have everything except IsActive.
-- ────────────────────────────────────────────────────────────────────────
SELECT t.name AS TableName,
  MAX(CASE WHEN c.name = 'CreatedBy' THEN 1 ELSE 0 END) AS CreatedBy,
  MAX(CASE WHEN c.name = 'CreatedAt' THEN 1 ELSE 0 END) AS CreatedAt,
  MAX(CASE WHEN c.name = 'UpdatedBy' THEN 1 ELSE 0 END) AS UpdatedBy,
  MAX(CASE WHEN c.name = 'UpdatedAt' THEN 1 ELSE 0 END) AS UpdatedAt,
  MAX(CASE WHEN c.name = 'DeletedBy' THEN 1 ELSE 0 END) AS DeletedBy,
  MAX(CASE WHEN c.name = 'DeletedAt' THEN 1 ELSE 0 END) AS DeletedAt,
  MAX(CASE WHEN c.name = 'IsActive' THEN 1 ELSE 0 END)  AS IsActive
FROM sys.tables t
LEFT JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.is_ms_shipped = 0
GROUP BY t.name
ORDER BY t.name;

PRINT '004_add_audit_columns.sql applied successfully.';
