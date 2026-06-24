-- =============================================================================
-- Migration 007 — seed global RoleAccess rows for Company roles
-- =============================================================================
-- Brings dev + prod to the intended global-access baseline.
--
--   SuperAdmin     → Information=2 (Edit), Access=2 (Edit)   ← already seeded
--   CompanyAdmin   → Information=2 (Edit), Access=2 (Edit)
--   CompanyViewer  → Information=1 (View), Access=1 (View)
--
-- CompanyMember is INTENTIONALLY NOT seeded here. Members get visibility only
-- via per-person rows in dbo.Access; they should not see every company in the
-- system by default.
--
-- Sections: 1=Information, 2=Access. Levels: 0=None, 1=View, 2=Edit.
--
-- Idempotent — each INSERT is guarded by NOT EXISTS, so this script is safe
-- to run multiple times and produces the same end state on any environment.
--
-- CreatedBy = 00000000-0000-0000-0000-000000000001 (system seed identity,
-- matches the existing SuperAdmin seed convention).
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

DECLARE @SystemSeed UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

-- CompanyAdmin × Information × Edit
IF NOT EXISTS (
    SELECT 1 FROM dbo.RoleAccess
    WHERE CompanyId IS NULL
      AND GrantedToRoleId = 'EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC'
      AND SectionId = 1
)
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
VALUES (NEWID(), NULL, 'EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC', 1, 2, @SystemSeed, @Now, 1);

-- CompanyAdmin × Access × Edit
IF NOT EXISTS (
    SELECT 1 FROM dbo.RoleAccess
    WHERE CompanyId IS NULL
      AND GrantedToRoleId = 'EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC'
      AND SectionId = 2
)
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
VALUES (NEWID(), NULL, 'EB26DCDC-AC63-4FA4-953C-11BF2EDB52BC', 2, 2, @SystemSeed, @Now, 1);

-- CompanyViewer × Information × View
IF NOT EXISTS (
    SELECT 1 FROM dbo.RoleAccess
    WHERE CompanyId IS NULL
      AND GrantedToRoleId = '019F1100-0000-7100-8000-000000000003'
      AND SectionId = 1
)
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
VALUES (NEWID(), NULL, '019F1100-0000-7100-8000-000000000003', 1, 1, @SystemSeed, @Now, 1);

-- CompanyViewer × Access × View
IF NOT EXISTS (
    SELECT 1 FROM dbo.RoleAccess
    WHERE CompanyId IS NULL
      AND GrantedToRoleId = '019F1100-0000-7100-8000-000000000003'
      AND SectionId = 2
)
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, [Level], CreatedBy, CreatedAt, IsActive)
VALUES (NEWID(), NULL, '019F1100-0000-7100-8000-000000000003', 2, 1, @SystemSeed, @Now, 1);

GO
