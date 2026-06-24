-- =============================================================================
-- Migration 010 — repair CompanyAdmin global RoleAccess rows in PROD
-- =============================================================================
-- The CompanyAdmin role has DIFFERENT GUIDs in dev vs prod (the seed used
-- NEWID() instead of a deterministic UUID for *Admin roles):
--
--   Dev  CompanyAdmin RoleId = EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC
--   Prod CompanyAdmin RoleId = 487BE343-EAAE-422A-9457-C2777C68A91E
--
-- Migration 007 (seed) hardcoded the DEV GUID and inserted it into BOTH DBs.
-- Result in prod: 2 RoleAccess rows referencing a role that doesn't exist
-- there, AND prod's real CompanyAdmin role has zero global RoleAccess.
--
-- This script branches on DB_NAME(). In prod it removes the wrong-GUID rows
-- and inserts the correct ones. In dev it is a no-op because the existing
-- rows already point at dev's real CompanyAdmin role.
--
-- Idempotent — safe to run multiple times.
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

DECLARE @SystemSeed UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @Now        DATETIME2        = SYSUTCDATETIME();

IF DB_NAME() = 'KSS_Company_Prod'
BEGIN
    PRINT N'Repairing CompanyAdmin global RoleAccess in PROD.';

    -- 1) Remove the wrong-GUID rows that migration 007 inserted using
    --    dev's CompanyAdmin GUID.
    DELETE FROM dbo.RoleAccess
    WHERE CompanyId IS NULL
      AND GrantedToRoleId = 'EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC';

    -- 2) Insert the correct rows using prod's real CompanyAdmin GUID.
    DECLARE @ProdCompanyAdminId UNIQUEIDENTIFIER = '487BE343-EAAE-422A-9457-C2777C68A91E';

    IF NOT EXISTS (
        SELECT 1 FROM dbo.RoleAccess
        WHERE CompanyId IS NULL
          AND GrantedToRoleId = @ProdCompanyAdminId
          AND SectionId = 1
    )
        INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
        VALUES (NEWID(), NULL, @ProdCompanyAdminId, 1, 2, @SystemSeed, @Now, 1);

    IF NOT EXISTS (
        SELECT 1 FROM dbo.RoleAccess
        WHERE CompanyId IS NULL
          AND GrantedToRoleId = @ProdCompanyAdminId
          AND SectionId = 2
    )
        INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
        VALUES (NEWID(), NULL, @ProdCompanyAdminId, 2, 2, @SystemSeed, @Now, 1);
END
ELSE IF DB_NAME() = 'KSS_Company_Dev'
BEGIN
    PRINT N'Dev environment detected — no repair needed (existing EB26DCDC rows are already correct for dev).';
END
ELSE
BEGIN
    DECLARE @Msg NVARCHAR(200) = N'Migration 010 expects DB_NAME() = KSS_Company_Dev or KSS_Company_Prod. Got: ' + DB_NAME();
    RAISERROR (@Msg, 16, 1);
END

GO
