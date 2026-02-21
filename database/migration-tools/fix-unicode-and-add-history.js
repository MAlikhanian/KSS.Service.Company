/**
 * Fix Unicode encoding in CompanyTranslation and add CompanyNameHistory records.
 *
 * Problem: sqlcmd inserted Persian names with broken UTF-8 encoding in NVARCHAR columns.
 * Solution: Use Node.js mssql driver with proper Unicode parameter binding.
 *
 * Also creates CompanyNameHistory + CompanyNameHistoryTranslation records for each company.
 *
 * Usage:
 *   Set DATABASE_URL env variable for PostgreSQL source, then:
 *   node fix-unicode-and-add-history.js
 *
 * Requires: npm install mssql @prisma/client
 */

const sql = require('mssql');
const { PrismaClient } = require('@prisma/client');
const crypto = require('crypto');

const sqlConfig = {
  user: 'kss',
  password: 'HZcg4!fjq0PQoO@%JFDn',
  database: 'KSS_Company_Prod',
  server: 'database91.sebaoffice.ir',
  options: {
    encrypt: false,
    trustServerCertificate: true,
  },
};

async function main() {
  // 1. Get brokerage data from PostgreSQL
  const prisma = new PrismaClient();
  const brokerages = await prisma.brokerage.findMany({
    where: {
      companyPersianName: { notIn: ['teat3', '\u062a\u0633\u062a19'] }
    },
    orderBy: { companyPersianName: 'asc' }
  });
  console.log(`Loaded ${brokerages.length} brokerages from PostgreSQL`);
  await prisma.$disconnect();

  // 2. Connect to SQL Server
  const pool = await sql.connect(sqlConfig);
  console.log('Connected to SQL Server');

  // 3. Start transaction
  const transaction = new sql.Transaction(pool);
  await transaction.begin();
  console.log('Transaction started');

  try {
    // 4. Delete existing CompanyNameHistory (if any) and CompanyTranslation
    console.log('Deleting existing CompanyNameHistoryTranslation...');
    await transaction.request().query('DELETE FROM dbo.CompanyNameHistoryTranslation');

    console.log('Deleting existing CompanyNameHistory...');
    await transaction.request().query('DELETE FROM dbo.CompanyNameHistory');

    console.log('Deleting existing CompanyTranslation...');
    await transaction.request().query('DELETE FROM dbo.CompanyTranslation');
    console.log('Old data cleared.');

    // 5. Re-insert CompanyTranslation with proper Unicode
    let persianCount = 0;
    let englishCount = 0;

    for (const b of brokerages) {
      // Persian translation (LanguageId = 12)
      const req1 = new sql.Request(transaction);
      req1.input('companyId', sql.UniqueIdentifier, b.id);
      req1.input('languageId', sql.SmallInt, 12);
      req1.input('name', sql.NVarChar(150), b.companyPersianName);
      await req1.query('INSERT INTO dbo.CompanyTranslation (CompanyId, LanguageId, Name) VALUES (@companyId, @languageId, @name)');
      persianCount++;

      // English translation (LanguageId = 10) if available
      if (b.companyLatinName) {
        const req2 = new sql.Request(transaction);
        req2.input('companyId', sql.UniqueIdentifier, b.id);
        req2.input('languageId', sql.SmallInt, 10);
        req2.input('name', sql.NVarChar(150), b.companyLatinName);
        await req2.query('INSERT INTO dbo.CompanyTranslation (CompanyId, LanguageId, Name) VALUES (@companyId, @languageId, @name)');
        englishCount++;
      }
    }
    console.log(`CompanyTranslation: ${persianCount} Persian + ${englishCount} English = ${persianCount + englishCount} total`);

    // 6. Insert CompanyNameHistory + CompanyNameHistoryTranslation
    // Each company gets one current name history record (EndDate = NULL)
    let historyCount = 0;
    let historyTransCount = 0;

    for (const b of brokerages) {
      // Use the company's registration date as StartDate, or CreatedAt, or 1900-01-01
      const startDate = b.registrationDate || b.seoRegistrationDate || new Date('1900-01-01');
      const startDateStr = startDate.toISOString().split('T')[0];

      // Insert CompanyNameHistory (generate UUID in Node since table has trigger, can't use OUTPUT)
      const historyId = crypto.randomUUID();
      const reqH = new sql.Request(transaction);
      reqH.input('historyId', sql.UniqueIdentifier, historyId);
      reqH.input('companyId', sql.UniqueIdentifier, b.id);
      reqH.input('startDate', sql.Date, startDateStr);
      await reqH.query(`
        INSERT INTO dbo.CompanyNameHistory (Id, CompanyId, StartDate, EndDate)
        VALUES (@historyId, @companyId, @startDate, NULL)
      `);
      historyCount++;

      // Insert CompanyNameHistoryTranslation - Persian
      const reqHT1 = new sql.Request(transaction);
      reqHT1.input('historyId', sql.UniqueIdentifier, historyId);
      reqHT1.input('languageId', sql.SmallInt, 12);
      reqHT1.input('name', sql.NVarChar(150), b.companyPersianName);
      await reqHT1.query('INSERT INTO dbo.CompanyNameHistoryTranslation (CompanyNameHistoryId, LanguageId, Name) VALUES (@historyId, @languageId, @name)');
      historyTransCount++;

      // Insert CompanyNameHistoryTranslation - English (if available)
      if (b.companyLatinName) {
        const reqHT2 = new sql.Request(transaction);
        reqHT2.input('historyId', sql.UniqueIdentifier, historyId);
        reqHT2.input('languageId', sql.SmallInt, 10);
        reqHT2.input('name', sql.NVarChar(150), b.companyLatinName);
        await reqHT2.query('INSERT INTO dbo.CompanyNameHistoryTranslation (CompanyNameHistoryId, LanguageId, Name) VALUES (@historyId, @languageId, @name)');
        historyTransCount++;
      }
    }
    console.log(`CompanyNameHistory: ${historyCount} records`);
    console.log(`CompanyNameHistoryTranslation: ${historyTransCount} records`);

    // 7. Commit
    await transaction.commit();
    console.log('');
    console.log('=== SUCCESS ===');
    console.log(`CompanyTranslation: ${persianCount + englishCount} records (fixed Unicode)`);
    console.log(`CompanyNameHistory: ${historyCount} records (EndDate = NULL = current)`);
    console.log(`CompanyNameHistoryTranslation: ${historyTransCount} records`);

  } catch (err) {
    await transaction.rollback();
    console.error('ERROR - Transaction rolled back:', err.message);
    throw err;
  } finally {
    await pool.close();
  }
}

main().catch(err => {
  console.error('Fatal error:', err);
  process.exit(1);
});
