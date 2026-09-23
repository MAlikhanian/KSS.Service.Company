-- =============================================================================
-- Migration 013 — Website + WebsiteLabel(+Translation)  (multi-value websites)
-- =============================================================================
-- Replaces the scalar Company.Website column with a multi-value Website child
-- collection, modeled on Email/Phone/Address:
--   WebsiteLabel             — tinyint identity lookup (Code + audit)
--   WebsiteLabelTranslation  — fa/en names (composite PK)
--   Website                  — one row per URL (GUID PK, label + IsPrimary)
-- Existing Company.Website values are migrated in as the PRIMARY website, then
-- the scalar column is dropped.
--
-- Guid ids on Website are normally app-generated (Guid v7); this one-time bulk
-- migration uses NEWID() for pre-existing rows (T-SQL has no v7 generator).
-- Lookup ids are DB-owned (IDENTITY). Language ids: fa = 12, en = 10.
--
-- Idempotent. Apply to KSS_Company_Dev first, then KSS_Company_Prod (with approval).
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 1) WebsiteLabel (lookup)
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.WebsiteLabel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WebsiteLabel (
        Id        TINYINT          IDENTITY(1, 1) NOT NULL,
        Code      VARCHAR(10)      NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2        NOT NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        UpdatedAt DATETIME2        NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2        NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_WebsiteLabel_IsActive DEFAULT (1),
        CONSTRAINT PK_WebsiteLabel PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_WebsiteLabel_Code UNIQUE (Code)
    );
    PRINT '013 — dbo.WebsiteLabel created.';
END
ELSE
    PRINT '013 — dbo.WebsiteLabel already exists, skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 2) WebsiteLabelTranslation (fa/en names)
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.WebsiteLabelTranslation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WebsiteLabelTranslation (
        WebsiteLabelId TINYINT          NOT NULL,
        LanguageId     SMALLINT         NOT NULL,
        Name           NVARCHAR(50)     NOT NULL,
        CreatedBy      UNIQUEIDENTIFIER NOT NULL,
        CreatedAt      DATETIME2        NOT NULL,
        UpdatedBy      UNIQUEIDENTIFIER NULL,
        UpdatedAt      DATETIME2        NULL,
        DeletedBy      UNIQUEIDENTIFIER NULL,
        DeletedAt      DATETIME2        NULL,
        CONSTRAINT PK_WebsiteLabelTranslation PRIMARY KEY CLUSTERED (WebsiteLabelId, LanguageId),
        CONSTRAINT FK_WebsiteLabelTranslation_Label
            FOREIGN KEY (WebsiteLabelId) REFERENCES dbo.WebsiteLabel (Id) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_WebsiteLabelTranslation_LanguageId
        ON dbo.WebsiteLabelTranslation (LanguageId);
    PRINT '013 — dbo.WebsiteLabelTranslation created.';
END
ELSE
    PRINT '013 — dbo.WebsiteLabelTranslation already exists, skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 3) Website (one row per URL; bytes n/a)
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.Website', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Website (
        Id        UNIQUEIDENTIFIER NOT NULL,
        CompanyId UNIQUEIDENTIFIER NOT NULL,
        LabelId   TINYINT          NOT NULL,
        Url       VARCHAR(256)     NOT NULL,
        IsPrimary BIT              NOT NULL CONSTRAINT DF_Website_IsPrimary DEFAULT (0),
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2        NOT NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        UpdatedAt DATETIME2        NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2        NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_Website_IsActive DEFAULT (1),
        CONSTRAINT PK_Website PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Website_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Website_Label   FOREIGN KEY (LabelId)   REFERENCES dbo.WebsiteLabel (Id),
        CONSTRAINT CK_Website_Url CHECK (Url NOT LIKE '% %' AND LEN(Url) >= 3)
    );
    CREATE NONCLUSTERED INDEX IX_Website_CompanyId ON dbo.Website (CompanyId);
    CREATE NONCLUSTERED INDEX IX_Website_LabelId  ON dbo.Website (LabelId);
    CREATE UNIQUE NONCLUSTERED INDEX UX_Website_Company_Url     ON dbo.Website (CompanyId, Url);
    CREATE UNIQUE NONCLUSTERED INDEX UX_Website_Company_Primary ON dbo.Website (CompanyId) WHERE IsPrimary = 1;
    PRINT '013 — dbo.Website created.';
END
ELSE
    PRINT '013 — dbo.Website already exists, skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 4) Seed WebsiteLabel (+ fa/en translations). Guarded — runs once.
-- ─────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.WebsiteLabel)
BEGIN
    DECLARE @sys UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
    DECLARE @now DATETIME2 = SYSUTCDATETIME();

    INSERT INTO dbo.WebsiteLabel (Code, CreatedBy, CreatedAt) VALUES
        ('Main',    @sys, @now),
        ('Support', @sys, @now),
        ('Shop',    @sys, @now),
        ('Careers', @sys, @now),
        ('Other',   @sys, @now);

    ;WITH v AS (
        SELECT Code, LanguageId, Name FROM (VALUES
            ('Main',    10, N'Main'),     ('Main',    12, N'اصلی'),
            ('Support', 10, N'Support'),  ('Support', 12, N'پشتیبانی'),
            ('Shop',    10, N'Shop'),     ('Shop',    12, N'فروشگاه'),
            ('Careers', 10, N'Careers'),  ('Careers', 12, N'استخدام'),
            ('Other',   10, N'Other'),    ('Other',   12, N'سایر')
        ) AS x(Code, LanguageId, Name)
    )
    INSERT INTO dbo.WebsiteLabelTranslation (WebsiteLabelId, LanguageId, Name, CreatedBy, CreatedAt)
    SELECT l.Id, v.LanguageId, v.Name, @sys, @now
    FROM v JOIN dbo.WebsiteLabel l ON l.Code = v.Code;

    PRINT '013 — WebsiteLabel seeded (5 labels, fa/en).';
END
ELSE
    PRINT '013 — WebsiteLabel already seeded, skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 5) Migrate existing Company.Website scalar → Website (primary). Guarded.
--    Malformed values (spaces, <3 chars) are skipped by the WHERE guard so the
--    CK_Website_Url check can't fail the batch.
-- ─────────────────────────────────────────────────────────────────────────
IF COL_LENGTH('dbo.Company', 'Website') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.Website)
BEGIN
    DECLARE @sysm UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
    DECLARE @nowm DATETIME2 = SYSUTCDATETIME();
    DECLARE @mainId TINYINT = (SELECT Id FROM dbo.WebsiteLabel WHERE Code = 'Main');

    INSERT INTO dbo.Website (Id, CompanyId, LabelId, Url, IsPrimary, CreatedBy, CreatedAt, IsActive)
    SELECT NEWID(), c.Id, @mainId, LTRIM(RTRIM(c.Website)), 1, @sysm, @nowm, 1
    FROM dbo.Company c
    WHERE c.Website IS NOT NULL
      AND LEN(LTRIM(RTRIM(c.Website))) >= 3
      AND LTRIM(RTRIM(c.Website)) NOT LIKE '% %';

    PRINT '013 — migrated existing Company.Website values into dbo.Website.';
END
ELSE
    PRINT '013 — Website migration skipped (column gone or table already populated).';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 6) Drop the scalar Company.Website column.
-- ─────────────────────────────────────────────────────────────────────────
IF COL_LENGTH('dbo.Company', 'Website') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Company DROP COLUMN Website;
    PRINT '013 — dropped scalar dbo.Company.Website column.';
END
ELSE
    PRINT '013 — dbo.Company.Website already dropped, skipped.';
GO
