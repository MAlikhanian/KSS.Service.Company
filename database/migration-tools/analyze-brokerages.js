/**
 * Analyze brokerage data in PostgreSQL before migration.
 *
 * Usage:
 *   Set DATABASE_URL env variable, then:
 *   node analyze-brokerages.js
 *
 * Requires: npm install @prisma/client
 * Note: Prisma schema must be generated from the KSS.Client.Web project.
 *       Run from KSS.Client.Web directory or set DATABASE_URL manually.
 */

const { PrismaClient } = require('@prisma/client');
const p = new PrismaClient();

async function main() {
  const r = await p.brokerage.findMany({
    where: {
      companyPersianName: { notIn: ['teat3', '\u062a\u0633\u062a19'] }
    },
    orderBy: { companyPersianName: 'asc' }
  });

  const total = r.length;
  const withRegNo = r.filter(x => x.registrationNumber).length;
  const withNatId = r.filter(x => x.nationalId).length;
  const withEconCode = r.filter(x => x.economicCode).length;
  const withRegDate = r.filter(x => x.registrationDate).length;
  const withLatinName = r.filter(x => x.companyLatinName).length;
  const withWebsite = r.filter(x => x.website).length;
  const withPhone = r.filter(x => x.phoneNumber).length;
  const withEmail = r.filter(x => x.email).length;
  const withAddress = r.filter(x => x.headOfficeAddress).length;
  const active = r.filter(x => x.status === 'ACTIVE').length;
  const suspended = r.filter(x => x.status === 'SUSPENDED').length;
  const inactive = r.filter(x => x.status === 'INACTIVE').length;

  const missingRegNo = r.filter(x => !x.registrationNumber);
  const missingNatId = r.filter(x => !x.nationalId);
  const missingEconCode = r.filter(x => !x.economicCode);
  const missingRegDate = r.filter(x => !x.registrationDate);

  const complete = r.filter(x => x.nationalId && x.registrationNumber && x.economicCode && x.registrationDate);
  const partial = r.filter(x => x.nationalId && (!x.registrationNumber || !x.economicCode || !x.registrationDate));

  console.log('=== BROKERAGE TRANSFER REPORT ===');
  console.log('Total records (excluding 2 test):', total);
  console.log('');
  console.log('--- Status ---');
  console.log('ACTIVE:', active);
  console.log('SUSPENDED:', suspended);
  console.log('INACTIVE:', inactive);
  console.log('');
  console.log('--- Data Completeness ---');
  console.log('Has NationalId:', withNatId, '/', total);
  console.log('Has RegistrationNo:', withRegNo, '/', total);
  console.log('Has EconomicCode:', withEconCode, '/', total);
  console.log('Has RegistrationDate:', withRegDate, '/', total);
  console.log('Has LatinName:', withLatinName, '/', total);
  console.log('Has Website:', withWebsite, '/', total);
  console.log('Has Phone:', withPhone, '/', total);
  console.log('Has Email:', withEmail, '/', total);
  console.log('Has Address:', withAddress, '/', total);
  console.log('');
  console.log('=== COMPANY TABLE REQUIRED FIELDS ANALYSIS ===');
  console.log('Company table requires: NationalId, RegistrationNo, EconomicCode, RegistrationDate, RegistrationCountryId, RegistrationRegionId, RegistrationCityId');
  console.log('');
  console.log('Missing NationalId:', missingNatId.length);
  console.log('Missing RegistrationNo:', missingRegNo.length);
  console.log('Missing EconomicCode:', missingEconCode.length);
  console.log('Missing RegistrationDate:', missingRegDate.length);
  console.log('');
  console.log('--- MIGRATION CATEGORIES ---');
  console.log('FULL DATA (all required fields present):', complete.length);
  console.log('PARTIAL DATA (has NationalId, missing others):', partial.length);
  console.log('');
  console.log('--- PARTIAL RECORDS (need defaults for missing required fields) ---');
  partial.forEach(x => {
    const m = [];
    if (!x.registrationNumber) m.push('RegistrationNo');
    if (!x.economicCode) m.push('EconomicCode');
    if (!x.registrationDate) m.push('RegistrationDate');
    console.log(' ', x.companyPersianName, '- missing:', m.join(', '));
  });

  console.log('');
  console.log('=== PROPOSED DEFAULTS FOR MISSING REQUIRED FIELDS ===');
  console.log("RegistrationNo (missing): Will use NationalId as placeholder");
  console.log("EconomicCode (missing): Will use NationalId as placeholder");
  console.log("RegistrationDate (missing): Will use '1900-01-01' as placeholder");
  console.log("RegistrationCountryId: 1 (Iran) for all records");
  console.log("RegistrationRegionId: 1 (Tehran) for all records (can be updated later)");
  console.log("RegistrationCityId: 1 (Tehran) for all records (can be updated later)");
  console.log("CompanyTypeId: 2 (Corporation/Joint Stock) for all brokerages");
  console.log("IndustryId: 2 (Finance & Banking) for all brokerages");

  await p.$disconnect();
}

main().catch(e => { console.error(e); p.$disconnect(); });
