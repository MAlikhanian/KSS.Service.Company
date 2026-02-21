/**
 * Generate SQL migration script to transfer brokerages from PostgreSQL to SQL Server.
 *
 * Usage:
 *   Set DATABASE_URL env variable, then:
 *   node generate-migration-sql.js
 *
 * Output: ../scripts/MigrateBrokerages_PostgreSQL_to_SqlServer.sql
 *
 * Requires: npm install @prisma/client
 * Note: Run from KSS.Client.Web directory or set DATABASE_URL manually.
 */

const { PrismaClient } = require('@prisma/client');
const fs = require('fs');
const path = require('path');
const p = new PrismaClient();

async function main() {
  const brokerages = await p.brokerage.findMany({
    where: {
      companyPersianName: { notIn: ['teat3', '\u062a\u0633\u062a19'] }
    },
    orderBy: { companyPersianName: 'asc' }
  });

  let sql = `-- ============================================================
-- Migration Script: Transfer Brokerages from PostgreSQL to KSS_Company_Prod
-- Generated: ${new Date().toISOString()}
-- Total records: ${brokerages.length} (2 test records excluded)
-- ============================================================
-- IMPORTANT: Run this on KSS_Company_Prod SQL Server database
-- ============================================================

USE [KSS_Company_Prod];
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Wrap in transaction for safety
BEGIN TRANSACTION;
BEGIN TRY

PRINT 'Starting brokerage migration...';
PRINT 'Inserting ${brokerages.length} brokerages into Company table...';

`;

  // Insert Company records
  for (const b of brokerages) {
    const id = b.id;
    const companyTypeId = 2;
    const industryId = 2;
    const regDate = b.registrationDate ? `'${b.registrationDate.toISOString().split('T')[0]}'` : "'1900-01-01'";
    const regNo = escSql(b.registrationNumber || b.nationalId);
    const nationalId = escSql(b.nationalId);
    const econCode = escSql(b.economicCode || b.nationalId);
    const countryId = 1;
    const regionId = 1;
    const cityId = 1;
    const website = b.website ? `'${escSql(b.website).substring(0, 256)}'` : 'NULL';
    const isActive = b.status === 'ACTIVE' ? 1 : 0;

    sql += `-- ${b.companyPersianName}
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('${id}', ${companyTypeId}, ${industryId}, ${regDate}, '${regNo}', '${nationalId}', '${econCode}', ${countryId}, ${regionId}, ${cityId}, ${website}, ${isActive});
`;
  }

  sql += `
PRINT 'Company records inserted successfully.';
PRINT 'Inserting CompanyTranslation records (Persian names)...';

`;

  for (const b of brokerages) {
    const name = escSql(b.companyPersianName).substring(0, 150);
    sql += `INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('${b.id}', 12, N'${name}');
`;
  }

  sql += `
PRINT 'Persian translations inserted.';
PRINT 'Inserting English translations where available...';

`;

  const withLatin = brokerages.filter(b => b.companyLatinName);
  for (const b of withLatin) {
    const name = escSql(b.companyLatinName).substring(0, 150);
    sql += `INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('${b.id}', 10, N'${name}');
`;
  }

  sql += `
PRINT 'English translations inserted (${withLatin.length} records).';

PRINT '';
PRINT '=== Migration Summary ===';
PRINT 'Companies inserted: ${brokerages.length}';
PRINT 'Persian translations: ${brokerages.length}';
PRINT 'English translations: ${withLatin.length}';
PRINT '=========================';

COMMIT TRANSACTION;
PRINT 'Migration completed successfully!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: Migration failed!';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
END CATCH;
GO
`;

  const outPath = path.join(__dirname, '..', 'scripts', 'MigrateBrokerages_PostgreSQL_to_SqlServer.sql');
  fs.writeFileSync(outPath, sql, 'utf8');
  console.log('SQL migration script generated successfully!');
  console.log('Output:', outPath);
  console.log('Total Company records:', brokerages.length);
  console.log('Total Persian translations:', brokerages.length);
  console.log('Total English translations:', withLatin.length);

  await p.$disconnect();
}

function escSql(str) {
  if (!str) return '';
  return str.replace(/'/g, "''");
}

main().catch(e => { console.error(e); p.$disconnect(); });
