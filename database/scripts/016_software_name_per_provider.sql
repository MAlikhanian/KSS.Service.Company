-- =============================================================================
-- Migration 016 — Software.Name unique PER PROVIDER (CompanyId, Name)
-- =============================================================================
-- Replaces the global UQ_Software_Name (from 014) with a per-provider unique
-- constraint, so two different provider companies can each have a same-named
-- software product (the provider->software cascade disambiguates them).
-- The Software table is empty in all envs, so the swap is zero-risk.
-- Idempotent. Apply to _Dev then _Prod (after 014 + 015).
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Software_Name' AND parent_object_id = OBJECT_ID('dbo.Software'))
BEGIN
    ALTER TABLE dbo.Software DROP CONSTRAINT UQ_Software_Name;
    PRINT '016 — dropped global UQ_Software_Name.';
END
ELSE PRINT '016 — UQ_Software_Name not present, skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Software_Company_Name' AND parent_object_id = OBJECT_ID('dbo.Software'))
BEGIN
    ALTER TABLE dbo.Software ADD CONSTRAINT UQ_Software_Company_Name UNIQUE (CompanyId, Name);
    PRINT '016 — added per-provider UQ_Software_Company_Name (CompanyId, Name).';
END
ELSE PRINT '016 — UQ_Software_Company_Name already exists, skipped.';
GO
