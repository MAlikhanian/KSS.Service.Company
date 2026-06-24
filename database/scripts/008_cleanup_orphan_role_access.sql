-- =============================================================================
-- Migration 008 — delete orphan global RoleAccess rows
-- =============================================================================
-- Two global rows reference role 487BE343-EAAE-422A-9457-C2777C68A91E, which
-- does NOT exist in KSS_Auth.dbo.Role. They are seed cruft — harmless at
-- runtime (no real user carries that role ID) but they should be removed so
-- the global-access baseline is clean.
--
-- Must run BEFORE migration 006 (trg_RoleAccess_ProtectGlobals), because that
-- trigger blocks deletes of global rows.
--
-- Idempotent — safe to run multiple times. If the rows are already gone, the
-- DELETE is a no-op.
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

DELETE FROM dbo.RoleAccess
WHERE CompanyId IS NULL
  AND GrantedToRoleId = '487BE343-EAAE-422A-9457-C2777C68A91E';

GO
