-- 003_rename_company_prefix_and_company_type_to_legalform.sql
--
-- Brings KSS_Company table names in line with the rest of the codebase
-- (Person, Auth, Members etc.). Inside a service's own DB, tables are not
-- prefixed by the service name — the DB / service context provides that.
-- Cross-service consumers (other DBs, frontend TS types) still use the
-- Company.* prefix when needed; that's a naming-convention concern, not a
-- DB-schema one.
--
-- Additionally renames CompanyType → LegalForm (and its column CompanyTypeId
-- → LegalFormId) because "Type" is a SQL/C# reserved-ish word AND the data
-- it actually classifies is the legal entity form (LLC, Corporation, etc.).
--
-- Apply to KSS_Company_Prod and KSS_Company_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- ───────────────────────────────────────────────────────────────────────
-- Step 1: Rename tables (8)
-- ───────────────────────────────────────────────────────────────────────
EXEC sp_rename N'dbo.CompanyTranslation',            N'Translation';
EXEC sp_rename N'dbo.CompanyNameHistory',             N'NameHistory';
EXEC sp_rename N'dbo.CompanyNameHistoryTranslation',  N'NameHistoryTranslation';
EXEC sp_rename N'dbo.CompanyFinancialInfo',           N'FinancialInfo';
EXEC sp_rename N'dbo.CompanyStakeholder',             N'Stakeholder';
EXEC sp_rename N'dbo.CompanyStakeholderHistory',      N'StakeholderHistory';
EXEC sp_rename N'dbo.CompanyType',                    N'LegalForm';
EXEC sp_rename N'dbo.CompanyTypeTranslation',         N'LegalFormTranslation';

-- ───────────────────────────────────────────────────────────────────────
-- Step 2: Rename columns whose semantics changed
-- ───────────────────────────────────────────────────────────────────────
-- Company.CompanyTypeId now points at LegalForm
EXEC sp_rename N'dbo.Company.CompanyTypeId',                       N'LegalFormId', N'COLUMN';
-- LegalFormTranslation FK column (was CompanyTypeId)
EXEC sp_rename N'dbo.LegalFormTranslation.CompanyTypeId',          N'LegalFormId', N'COLUMN';
-- NameHistoryTranslation FK column (was CompanyNameHistoryId)
EXEC sp_rename N'dbo.NameHistoryTranslation.CompanyNameHistoryId', N'NameHistoryId', N'COLUMN';
-- CompanyId columns (FKs OUT to dbo.Company) keep their names — they point
-- to a sibling table, not the current table, so the prefix still adds info.

-- ───────────────────────────────────────────────────────────────────────
-- Step 3: Rename PK / UQ constraints (12)
-- ───────────────────────────────────────────────────────────────────────
EXEC sp_rename N'dbo.PK_CompanyTranslation',                   N'PK_Translation',                   N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyNameHistory',                   N'PK_NameHistory',                   N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyNameHistoryTranslation',        N'PK_NameHistoryTranslation',        N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyFinancialInfo',                 N'PK_FinancialInfo',                 N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyStakeholder',                   N'PK_Stakeholder',                   N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyStakeholderHistory',            N'PK_StakeholderHistory',            N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyType',                          N'PK_LegalForm',                     N'OBJECT';
EXEC sp_rename N'dbo.PK_CompanyTypeTranslation',               N'PK_LegalFormTranslation',          N'OBJECT';
EXEC sp_rename N'dbo.UQ_CompanyType_Code',                     N'UQ_LegalForm_Code',                N'OBJECT';
EXEC sp_rename N'dbo.UX_CompanyFinancialInfo_Company_Year',    N'UX_FinancialInfo_Company_Year',    N'OBJECT';
EXEC sp_rename N'dbo.UQ_CompanyStakeholder',                   N'UQ_Stakeholder',                   N'OBJECT';

-- ───────────────────────────────────────────────────────────────────────
-- Step 4: Rename FK constraints (10)
-- ───────────────────────────────────────────────────────────────────────
EXEC sp_rename N'dbo.FK_Company_CompanyType',                  N'FK_Company_LegalForm',                  N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyTranslation_Company',           N'FK_Translation_Company',                N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyNameHistory_Company',           N'FK_NameHistory_Company',                N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyNameHistoryTranslation_History', N'FK_NameHistoryTranslation_NameHistory', N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyFinancialInfo_Company',         N'FK_FinancialInfo_Company',              N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyStakeholder_Company',           N'FK_Stakeholder_Company',                N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyStakeholder_Type',              N'FK_Stakeholder_StakeholderType',        N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyStakeholderHistory_Stakeholder', N'FK_StakeholderHistory_Stakeholder',     N'OBJECT';
EXEC sp_rename N'dbo.FK_CompanyTypeTranslation_CompanyType',   N'FK_LegalFormTranslation_LegalForm',     N'OBJECT';
-- FK_Company_Industry stays as-is (Industry table unchanged)

-- ───────────────────────────────────────────────────────────────────────
-- Step 5: Rename non-unique / unique indexes that include renamed names
-- ───────────────────────────────────────────────────────────────────────
-- Company.* indexes whose names mention CompanyTypeId
EXEC sp_rename N'dbo.Company.IX_Company_CompanyTypeId',                N'IX_Company_LegalFormId',              N'INDEX';

-- Indexes on renamed tables — names start with the old table name
EXEC sp_rename N'dbo.FinancialInfo.IX_CompanyFinancialInfo_CompanyId',  N'IX_FinancialInfo_CompanyId',  N'INDEX';
EXEC sp_rename N'dbo.FinancialInfo.IX_CompanyFinancialInfo_FiscalYear', N'IX_FinancialInfo_FiscalYear', N'INDEX';

EXEC sp_rename N'dbo.NameHistory.IX_CompanyNameHistory_CompanyId', N'IX_NameHistory_CompanyId', N'INDEX';
EXEC sp_rename N'dbo.NameHistory.IX_CompanyNameHistory_EndDate',   N'IX_NameHistory_EndDate',   N'INDEX';
EXEC sp_rename N'dbo.NameHistory.IX_CompanyNameHistory_StartDate', N'IX_NameHistory_StartDate', N'INDEX';
EXEC sp_rename N'dbo.NameHistory.UX_CompanyNameHistory_Current',   N'UX_NameHistory_Current',   N'INDEX';

EXEC sp_rename N'dbo.NameHistoryTranslation.IX_CompanyNameHistoryTranslation_LanguageId', N'IX_NameHistoryTranslation_LanguageId', N'INDEX';
EXEC sp_rename N'dbo.NameHistoryTranslation.IX_CompanyNameHistoryTranslation_Name',       N'IX_NameHistoryTranslation_Name',       N'INDEX';

EXEC sp_rename N'dbo.Stakeholder.IX_CompanyStakeholder_CompanyId',          N'IX_Stakeholder_CompanyId',          N'INDEX';
EXEC sp_rename N'dbo.Stakeholder.IX_CompanyStakeholder_RelatedParty',       N'IX_Stakeholder_RelatedParty',       N'INDEX';
EXEC sp_rename N'dbo.Stakeholder.IX_CompanyStakeholder_StakeholderTypeId',  N'IX_Stakeholder_StakeholderTypeId',  N'INDEX';

EXEC sp_rename N'dbo.StakeholderHistory.IX_CompanyStakeholderHistory_BoardRepresentativePersonId', N'IX_StakeholderHistory_BoardRepresentativePersonId', N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.IX_CompanyStakeholderHistory_CompanyStakeholderId',        N'IX_StakeholderHistory_StakeholderId',               N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.IX_CompanyStakeholderHistory_EffectiveDate',               N'IX_StakeholderHistory_EffectiveDate',               N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.IX_CompanyStakeholderHistory_EndDate',                     N'IX_StakeholderHistory_EndDate',                     N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.IX_CompanyStakeholderHistory_RegistrationDate',            N'IX_StakeholderHistory_RegistrationDate',            N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.UX_CompanyStakeholderHistory_Current',                     N'UX_StakeholderHistory_Current',                     N'INDEX';
EXEC sp_rename N'dbo.StakeholderHistory.UX_CompanyStakeholderHistory_Stakeholder_EffectiveDate',   N'UX_StakeholderHistory_Stakeholder_EffectiveDate',   N'INDEX';

EXEC sp_rename N'dbo.Translation.IX_CompanyTranslation_LanguageId_Name', N'IX_Translation_LanguageId_Name', N'INDEX';
EXEC sp_rename N'dbo.Translation.IX_CompanyTranslation_Name',            N'IX_Translation_Name',            N'INDEX';

EXEC sp_rename N'dbo.LegalFormTranslation.IX_CompanyTypeTranslation_LanguageId', N'IX_LegalFormTranslation_LanguageId', N'INDEX';

-- The renamed StakeholderHistory references a CompanyStakeholderId column
-- (the FK to Stakeholder). That column name remains CompanyStakeholderId
-- intentionally (FK out to Stakeholder which IS the renamed table, but the
-- column was historically named after the original full name). Leaving it
-- means existing C# property mappings (CompanyStakeholderId in entity)
-- still work — frontend never reads this column. If you want to rename it
-- to StakeholderId, do that in a follow-up.

COMMIT TRANSACTION;

-- ───────────────────────────────────────────────────────────────────────
-- Verification
-- ───────────────────────────────────────────────────────────────────────
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE 'Company%'
ORDER BY TABLE_NAME;
-- Should return only: Company (the base table; everything else now lacks the prefix)

SELECT TOP 20 c.name AS [Column], t.name AS [Table]
FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
WHERE c.name IN ('CompanyTypeId','CompanyNameHistoryId')
ORDER BY t.name, c.name;
-- Should return zero rows.

PRINT '003_rename_company_prefix_and_company_type_to_legalform.sql applied successfully.';
