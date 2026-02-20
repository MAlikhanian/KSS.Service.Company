-- ============================================================
-- Database: KSS_Company_Prod (microservice: company)
-- Delete data from data tables only (child-before-parent order). Lookup/seed tables are left intact.
-- Uses DELETE so FK-referenced parents (Company, Address, etc.) can be cleared; TRUNCATE would fail on them.
-- ============================================================
USE [KSS_Company_Prod];
GO

-- Data tables only: children before parents.
-- Section 4: Stakeholder history then stakeholder
DELETE FROM dbo.[CompanyStakeholderHistory];
DELETE FROM dbo.[CompanyStakeholder];
-- Section 3: Name history translations then name history, then company translations
DELETE FROM dbo.[CompanyNameHistoryTranslation];
DELETE FROM dbo.[CompanyNameHistory];
DELETE FROM dbo.[CompanyTranslation];
-- Section 5: Contact data (address translation, address, phone, email)
DELETE FROM dbo.[AddressTranslation];
DELETE FROM dbo.[Address];
DELETE FROM dbo.[Phone];
DELETE FROM dbo.[Email];
-- Section 2: Company (main entity)
DELETE FROM dbo.[Company];
GO
