-- =============================================================================
-- Migration 006 — protect global RoleAccess rows
-- =============================================================================
-- Global RoleAccess rows (CompanyId IS NULL) define system-wide visibility for
-- admin-style roles (SuperAdmin, CompanyAdmin, etc.). Losing them locks every
-- non-admin user out of the company list. They MUST not be deleted at runtime.
--
-- The application-layer guard lives in RoleAccessService.UpsertGrantAsync /
-- RevokeByPairAsync. This trigger is a defensive second layer that catches
-- raw SQL or any bypass of the service layer.
--
-- The trigger only blocks DELETEs targeting global rows. Per-company rows
-- (CompanyId IS NOT NULL) delete normally.
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.trg_RoleAccess_ProtectGlobals', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_RoleAccess_ProtectGlobals;
GO

CREATE TRIGGER dbo.trg_RoleAccess_ProtectGlobals
ON dbo.RoleAccess
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted WHERE CompanyId IS NULL)
    BEGIN
        RAISERROR ('Global RoleAccess rows (CompanyId IS NULL) are immutable — manage via versioned migrations only.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -- Per-company rows pass through.
    DELETE r
    FROM dbo.RoleAccess r
    INNER JOIN deleted d ON r.Id = d.Id;
END;
GO

-- Optional: also protect against UPDATEs that would null-out CompanyId
-- or change a global row's grant target / level — uncomment if you want
-- the strictest interpretation.
--
-- IF OBJECT_ID(N'dbo.trg_RoleAccess_ProtectGlobalUpdates', N'TR') IS NOT NULL
--     DROP TRIGGER dbo.trg_RoleAccess_ProtectGlobalUpdates;
-- GO
-- CREATE TRIGGER dbo.trg_RoleAccess_ProtectGlobalUpdates
-- ON dbo.RoleAccess
-- INSTEAD OF UPDATE
-- AS
-- BEGIN
--     SET NOCOUNT ON;
--     IF EXISTS (
--         SELECT 1
--         FROM deleted d
--         INNER JOIN inserted i ON d.Id = i.Id
--         WHERE d.CompanyId IS NULL
--     )
--     BEGIN
--         RAISERROR ('Global RoleAccess rows are immutable — manage via versioned migrations only.', 16, 1);
--         ROLLBACK TRANSACTION;
--         RETURN;
--     END;
--     UPDATE r SET
--         CompanyId       = i.CompanyId,
--         GrantedToRoleId = i.GrantedToRoleId,
--         SectionId       = i.SectionId,
--         [Level]         = i.[Level],
--         CreatedBy       = i.CreatedBy,
--         CreatedAt       = i.CreatedAt,
--         UpdatedBy       = i.UpdatedBy,
--         UpdatedAt       = i.UpdatedAt,
--         DeletedBy       = i.DeletedBy,
--         DeletedAt       = i.DeletedAt,
--         IsActive        = i.IsActive
--     FROM dbo.RoleAccess r
--     INNER JOIN inserted i ON r.Id = i.Id;
-- END;
-- GO
