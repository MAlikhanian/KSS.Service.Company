-- =============================================================================
-- Migration 015 — add Software.CompanyId (provider / vendor company)
-- =============================================================================
-- Each Software is made by a provider Company. Additive; the Software table is
-- empty in all envs so the NOT NULL column is safe. FK is NO ACTION (default) to
-- avoid a multiple-cascade-path conflict with CompanySoftware->Company (cascade).
-- Idempotent. Apply to _Dev (already has 014) then _Prod (after 014).
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.Software', 'CompanyId') IS NULL
BEGIN
    ALTER TABLE dbo.Software ADD CompanyId UNIQUEIDENTIFIER NOT NULL;
    PRINT '015 — dbo.Software.CompanyId added.';
END
ELSE PRINT '015 — dbo.Software.CompanyId already exists, skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Software_Company')
BEGIN
    ALTER TABLE dbo.Software
        ADD CONSTRAINT FK_Software_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (Id);
    PRINT '015 — FK_Software_Company added.';
END
ELSE PRINT '015 — FK_Software_Company already exists, skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Software_CompanyId' AND object_id = OBJECT_ID('dbo.Software'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Software_CompanyId ON dbo.Software (CompanyId);
    PRINT '015 — IX_Software_CompanyId created.';
END
ELSE PRINT '015 — IX_Software_CompanyId already exists, skipped.';
GO
