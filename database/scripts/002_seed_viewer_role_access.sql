-- 002_seed_viewer_role_access.sql
--
-- Seeds global RoleAccess rows for CompanyViewer so that role sees every
-- company in read-only mode. Pattern mirrors migration 001 which seeded
-- SuperAdmin + CompanyAdmin at Level=2; this seeds CompanyViewer at Level=1.
--
-- 2 rows: CompanyId=NULL, GrantedToRoleId=CompanyViewer, Level=1, one per section
-- (Information, Access).
--
-- Companion to Auth migration 011 which creates the CompanyViewer role.
--
-- Apply to KSS_Company_Prod and KSS_Company_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();

DECLARE @authDb SYSNAME =
    CASE WHEN DB_NAME() = 'KSS_Company_Prod' THEN N'KSS_Auth_Prod'
         ELSE N'KSS_Auth_Dev'
    END;
DECLARE @sql NVARCHAR(MAX);

BEGIN TRANSACTION;

-- Resolve CompanyViewer role Id from Auth DB
DECLARE @companyViewerId UNIQUEIDENTIFIER;
SET @sql = N'SELECT @id = Id FROM ' + QUOTENAME(@authDb) + N'.dbo.[Role] WHERE Code = ''CompanyViewer''';
EXEC sp_executesql @sql, N'@id UNIQUEIDENTIFIER OUTPUT', @id = @companyViewerId OUTPUT;

IF @companyViewerId IS NULL
    THROW 51060, 'CompanyViewer role not found in Auth DB — run Auth migration 011 first', 1;

-- Insert 2 RoleAccess rows (1 per section) at Level=1 (View) — idempotent
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, Level, CreatedBy, CreatedAt)
SELECT NEWID(), NULL, @companyViewerId, s.SectionId, 1, @system, @now
FROM (VALUES (1), (2)) AS s(SectionId)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RoleAccess ra
    WHERE ra.GrantedToRoleId = @companyViewerId
      AND ra.CompanyId IS NULL
      AND ra.SectionId = s.SectionId
);

COMMIT TRANSACTION;

SELECT ra.GrantedToRoleId, ra.SectionId, ra.Level, ra.CompanyId
FROM dbo.RoleAccess ra
WHERE ra.GrantedToRoleId = @companyViewerId
ORDER BY ra.SectionId;

PRINT '002_seed_viewer_role_access.sql applied successfully.';
