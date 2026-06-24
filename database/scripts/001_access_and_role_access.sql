-- 001_access_and_role_access.sql
--
-- Adds the section-level access tables for the new /company/* pages,
-- mirroring the Person service's AccessSection / Access / RoleAccess
-- schema. Two sections only: Information (1) and Access (2).
--
--   • AccessSection             — lookup, 2 fixed rows
--   • AccessSectionTranslation  — fa + en names per section
--   • Access                    — per-(Company, GrantedToPerson, Section) row, Level 0..2
--   • RoleAccess                — per-(Company OR NULL, GrantedToRole, Section) row, Level 0..2
--                                 CompanyId NULL = grant applies to all companies
--
-- Seeds: SuperAdmin + CompanyAdmin get global Edit on both sections (4 rows
-- each = 4 total). Role Ids are cross-DB; resolved by Code at migration time.
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

-- ── Step 1: AccessSection lookup
CREATE TABLE dbo.AccessSection (
    Id          TINYINT          NOT NULL,
    Code        VARCHAR(20)      NOT NULL,
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    DeletedBy   UNIQUEIDENTIFIER NULL,
    DeletedAt   DATETIME2        NULL,
    IsActive    BIT              NOT NULL CONSTRAINT DF_AccessSection_IsActive DEFAULT (1),
    CONSTRAINT PK_AccessSection PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_AccessSection_Code UNIQUE (Code)
);

INSERT INTO dbo.AccessSection (Id, Code, CreatedBy, CreatedAt, IsActive) VALUES
    (1, 'information', @system, @now, 1),
    (2, 'access',      @system, @now, 1);

-- ── Step 2: AccessSectionTranslation
CREATE TABLE dbo.AccessSectionTranslation (
    AccessSectionId TINYINT          NOT NULL,
    LanguageId      SMALLINT         NOT NULL,
    Name            NVARCHAR(50)     NOT NULL,
    CreatedBy       UNIQUEIDENTIFIER NOT NULL,
    CreatedAt       DATETIME2        NOT NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedAt       DATETIME2        NULL,
    DeletedBy       UNIQUEIDENTIFIER NULL,
    DeletedAt       DATETIME2        NULL,
    CONSTRAINT PK_AccessSectionTranslation PRIMARY KEY CLUSTERED (AccessSectionId, LanguageId),
    CONSTRAINT FK_AccessSectionTranslation_AccessSection FOREIGN KEY (AccessSectionId)
        REFERENCES dbo.AccessSection(Id) ON DELETE CASCADE
);

INSERT INTO dbo.AccessSectionTranslation (AccessSectionId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    (1, 12, N'اطلاعات شرکت', @system, @now),
    (1, 10, N'Information',  @system, @now),
    (2, 12, N'دسترسی‌ها',     @system, @now),
    (2, 10, N'Access',       @system, @now);

-- ── Step 3: Access table (per-person grants on a company)
CREATE TABLE dbo.Access (
    Id                UNIQUEIDENTIFIER NOT NULL,
    CompanyId         UNIQUEIDENTIFIER NOT NULL,
    GrantedToPersonId UNIQUEIDENTIFIER NOT NULL,
    SectionId         TINYINT          NOT NULL,
    Level             INT              NOT NULL,
    CreatedBy         UNIQUEIDENTIFIER NOT NULL,
    CreatedAt         DATETIME2        NOT NULL,
    UpdatedBy         UNIQUEIDENTIFIER NULL,
    UpdatedAt         DATETIME2        NULL,
    DeletedBy         UNIQUEIDENTIFIER NULL,
    DeletedAt         DATETIME2        NULL,
    IsActive          BIT              NOT NULL CONSTRAINT DF_Access_IsActive DEFAULT (1),
    CONSTRAINT PK_Access PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Access_AccessSection FOREIGN KEY (SectionId) REFERENCES dbo.AccessSection(Id),
    CONSTRAINT CK_Access_Level CHECK (Level BETWEEN 0 AND 2),
    CONSTRAINT UQ_Access UNIQUE (CompanyId, GrantedToPersonId, SectionId)
);

CREATE NONCLUSTERED INDEX IX_Access_CompanyId         ON dbo.Access (CompanyId);
CREATE NONCLUSTERED INDEX IX_Access_GrantedToPersonId ON dbo.Access (GrantedToPersonId);

-- ── Step 4: RoleAccess table (per-role grants; CompanyId NULL = applies to all)
CREATE TABLE dbo.RoleAccess (
    Id                UNIQUEIDENTIFIER NOT NULL,
    CompanyId         UNIQUEIDENTIFIER NULL,
    GrantedToRoleId   UNIQUEIDENTIFIER NOT NULL,
    SectionId         TINYINT          NOT NULL,
    Level             INT              NOT NULL,
    CreatedBy         UNIQUEIDENTIFIER NOT NULL,
    CreatedAt         DATETIME2        NOT NULL,
    UpdatedBy         UNIQUEIDENTIFIER NULL,
    UpdatedAt         DATETIME2        NULL,
    DeletedBy         UNIQUEIDENTIFIER NULL,
    DeletedAt         DATETIME2        NULL,
    IsActive          BIT              NOT NULL CONSTRAINT DF_RoleAccess_IsActive DEFAULT (1),
    CONSTRAINT PK_RoleAccess PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_RoleAccess_AccessSection FOREIGN KEY (SectionId) REFERENCES dbo.AccessSection(Id),
    CONSTRAINT CK_RoleAccess_Level CHECK (Level BETWEEN 0 AND 2)
);

CREATE UNIQUE NONCLUSTERED INDEX UQ_RoleAccess_PerCompany
    ON dbo.RoleAccess (CompanyId, GrantedToRoleId, SectionId)
    WHERE CompanyId IS NOT NULL;

CREATE UNIQUE NONCLUSTERED INDEX UQ_RoleAccess_Global
    ON dbo.RoleAccess (GrantedToRoleId, SectionId)
    WHERE CompanyId IS NULL;

CREATE NONCLUSTERED INDEX IX_RoleAccess_CompanyId ON dbo.RoleAccess (CompanyId);
CREATE NONCLUSTERED INDEX IX_RoleAccess_RoleId    ON dbo.RoleAccess (GrantedToRoleId);

-- ── Step 5: Resolve SuperAdmin + CompanyAdmin role Ids from Auth DB
DECLARE @superAdminId   UNIQUEIDENTIFIER;
DECLARE @companyAdminId UNIQUEIDENTIFIER;

SET @sql = N'SELECT @sa = (SELECT Id FROM ' + QUOTENAME(@authDb) + N'.dbo.[Role] WHERE Code = ''SuperAdmin''),
                  @ca = (SELECT Id FROM ' + QUOTENAME(@authDb) + N'.dbo.[Role] WHERE Code = ''CompanyAdmin'')';
EXEC sp_executesql @sql,
    N'@sa UNIQUEIDENTIFIER OUTPUT, @ca UNIQUEIDENTIFIER OUTPUT',
    @sa = @superAdminId OUTPUT, @ca = @companyAdminId OUTPUT;

IF @superAdminId IS NULL OR @companyAdminId IS NULL
BEGIN
    THROW 51030, 'SuperAdmin or CompanyAdmin role not found in Auth DB', 1;
END

-- ── Step 6: Seed global Edit grants for SuperAdmin + CompanyAdmin
-- 2 roles × 2 sections × Level=2 (Edit), all with CompanyId = NULL
INSERT INTO dbo.RoleAccess (Id, CompanyId, GrantedToRoleId, SectionId, Level, CreatedBy, CreatedAt) VALUES
    (NEWID(), NULL, @superAdminId,   1, 2, @system, @now),
    (NEWID(), NULL, @superAdminId,   2, 2, @system, @now),
    (NEWID(), NULL, @companyAdminId, 1, 2, @system, @now),
    (NEWID(), NULL, @companyAdminId, 2, 2, @system, @now);

COMMIT TRANSACTION;

SELECT 'access_section',         COUNT(*) FROM dbo.AccessSection;
SELECT 'access_section_trans',   COUNT(*) FROM dbo.AccessSectionTranslation;
SELECT 'access',                 COUNT(*) FROM dbo.Access;
SELECT 'role_access',            COUNT(*) FROM dbo.RoleAccess;

SELECT ra.GrantedToRoleId, ra.SectionId, ra.Level, ra.CompanyId
FROM dbo.RoleAccess ra
ORDER BY ra.GrantedToRoleId, ra.SectionId;

PRINT '001_access_and_role_access.sql applied successfully.';
