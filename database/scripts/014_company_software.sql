-- =============================================================================
-- Migration 014 — SoftwareCategory(+Translation) + Software + CompanySoftware
-- =============================================================================
-- Company (brokerage) software-by-category. SoftwareCategory is a seeded lookup
-- (9 functional slots); Software is an admin-managed flat product list;
-- CompanySoftware records one Software per (Company, Category).
-- Additive only — no column drops. Idempotent. Apply to _Dev then _Prod.
-- Language ids: fa=12, en=10.
-- =============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1) SoftwareCategory (lookup)
IF OBJECT_ID(N'dbo.SoftwareCategory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SoftwareCategory (
        Id        TINYINT          IDENTITY(1,1) NOT NULL,
        Code      VARCHAR(10)      NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2        NOT NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        UpdatedAt DATETIME2        NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2        NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_SoftwareCategory_IsActive DEFAULT (1),
        CONSTRAINT PK_SoftwareCategory PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_SoftwareCategory_Code UNIQUE (Code)
    );
    PRINT '014 — dbo.SoftwareCategory created.';
END ELSE PRINT '014 — dbo.SoftwareCategory exists, skipped.';
GO

-- 2) SoftwareCategoryTranslation (fa/en)
IF OBJECT_ID(N'dbo.SoftwareCategoryTranslation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SoftwareCategoryTranslation (
        SoftwareCategoryId TINYINT          NOT NULL,
        LanguageId         SMALLINT         NOT NULL,
        Name               NVARCHAR(100)    NOT NULL,
        CreatedBy          UNIQUEIDENTIFIER NOT NULL,
        CreatedAt          DATETIME2        NOT NULL,
        UpdatedBy          UNIQUEIDENTIFIER NULL,
        UpdatedAt          DATETIME2        NULL,
        DeletedBy          UNIQUEIDENTIFIER NULL,
        DeletedAt          DATETIME2        NULL,
        CONSTRAINT PK_SoftwareCategoryTranslation PRIMARY KEY CLUSTERED (SoftwareCategoryId, LanguageId),
        CONSTRAINT FK_SoftwareCategoryTranslation_Category
            FOREIGN KEY (SoftwareCategoryId) REFERENCES dbo.SoftwareCategory (Id) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_SoftwareCategoryTranslation_LanguageId
        ON dbo.SoftwareCategoryTranslation (LanguageId);
    PRINT '014 — dbo.SoftwareCategoryTranslation created.';
END ELSE PRINT '014 — dbo.SoftwareCategoryTranslation exists, skipped.';
GO

-- 3) Software (admin-managed product list)
IF OBJECT_ID(N'dbo.Software', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Software (
        Id        INT              IDENTITY(1,1) NOT NULL,
        Name      NVARCHAR(150)    NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2        NOT NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        UpdatedAt DATETIME2        NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2        NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_Software_IsActive DEFAULT (1),
        CONSTRAINT PK_Software PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Software_Name UNIQUE (Name)
    );
    PRINT '014 — dbo.Software created.';
END ELSE PRINT '014 — dbo.Software exists, skipped.';
GO

-- 4) CompanySoftware (junction; one Software per Company+Category)
IF OBJECT_ID(N'dbo.CompanySoftware', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CompanySoftware (
        Id                 UNIQUEIDENTIFIER NOT NULL,
        CompanyId          UNIQUEIDENTIFIER NOT NULL,
        SoftwareCategoryId TINYINT          NOT NULL,
        SoftwareId         INT              NOT NULL,
        CreatedBy          UNIQUEIDENTIFIER NOT NULL,
        CreatedAt          DATETIME2        NOT NULL,
        UpdatedBy          UNIQUEIDENTIFIER NULL,
        UpdatedAt          DATETIME2        NULL,
        DeletedBy          UNIQUEIDENTIFIER NULL,
        DeletedAt          DATETIME2        NULL,
        IsActive           BIT              NOT NULL CONSTRAINT DF_CompanySoftware_IsActive DEFAULT (1),
        CONSTRAINT PK_CompanySoftware PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CompanySoftware_Company  FOREIGN KEY (CompanyId)          REFERENCES dbo.Company (Id) ON DELETE CASCADE,
        CONSTRAINT FK_CompanySoftware_Category FOREIGN KEY (SoftwareCategoryId) REFERENCES dbo.SoftwareCategory (Id),
        CONSTRAINT FK_CompanySoftware_Software FOREIGN KEY (SoftwareId)         REFERENCES dbo.Software (Id)
    );
    CREATE NONCLUSTERED INDEX IX_CompanySoftware_CompanyId ON dbo.CompanySoftware (CompanyId);
    CREATE UNIQUE NONCLUSTERED INDEX UX_CompanySoftware_Company_Category ON dbo.CompanySoftware (CompanyId, SoftwareCategoryId);
    PRINT '014 — dbo.CompanySoftware created.';
END ELSE PRINT '014 — dbo.CompanySoftware exists, skipped.';
GO

-- 5) Seed the 9 SoftwareCategory rows (+ fa/en). Guarded.
IF NOT EXISTS (SELECT 1 FROM dbo.SoftwareCategory)
BEGIN
    DECLARE @sys UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
    DECLARE @now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.SoftwareCategory (Code, CreatedBy, CreatedAt) VALUES
        ('BOSEC',@sys,@now),('BOCOM',@sys,@now),('OTSEC',@sys,@now),
        ('OTFSEC',@sys,@now),('OTFCOM',@sys,@now),('ACC',@sys,@now),
        ('PMS',@sys,@now),('CMS',@sys,@now),('CALL',@sys,@now);

    ;WITH v AS (
        SELECT Code, LanguageId, Name FROM (VALUES
            ('BOSEC',10,N'Back Office (Securities Exchange)'), ('BOSEC',12,N'بک‌آفیس بورس اوراق بهادار'),
            ('BOCOM',10,N'Back Office (Commodities Exchange)'),('BOCOM',12,N'بک‌آفیس بورس کالا'),
            ('OTSEC',10,N'Online Trading (Securities)'),       ('OTSEC',12,N'معاملات آنلاین اوراق بهادار'),
            ('OTFSEC',10,N'Online Trading (Futures - Securities)'),('OTFSEC',12,N'معاملات آنلاین آتی اوراق'),
            ('OTFCOM',10,N'Online Trading (Futures - Commodities)'),('OTFCOM',12,N'معاملات آنلاین آتی کالا'),
            ('ACC',10,N'Accounting System'),                  ('ACC',12,N'سیستم حسابداری'),
            ('PMS',10,N'Portfolio Management Software'),       ('PMS',12,N'نرم‌افزار مدیریت پرتفوی'),
            ('CMS',10,N'Website CMS'),                         ('CMS',12,N'سیستم مدیریت محتوای وب‌سایت'),
            ('CALL',10,N'Call Center System'),                ('CALL',12,N'سیستم مرکز تماس')
        ) AS x(Code, LanguageId, Name)
    )
    INSERT INTO dbo.SoftwareCategoryTranslation (SoftwareCategoryId, LanguageId, Name, CreatedBy, CreatedAt)
    SELECT sc.Id, v.LanguageId, v.Name, @sys, @now
    FROM v JOIN dbo.SoftwareCategory sc ON sc.Code = v.Code;
    PRINT '014 — SoftwareCategory seeded (9, fa/en).';
END ELSE PRINT '014 — SoftwareCategory already seeded, skipped.';
GO
