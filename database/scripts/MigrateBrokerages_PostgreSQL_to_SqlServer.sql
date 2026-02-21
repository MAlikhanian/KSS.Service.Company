-- ============================================================
-- Migration Script: Transfer Brokerages from PostgreSQL to KSS_Company_Prod
-- Generated: 2026-02-21T06:33:17.320Z
-- Total records: 121 (2 test records excluded)
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
PRINT 'Inserting 121 brokerages into Company table...';

-- شرکت کارگزاری توازن بازار
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7c4fb83f-133c-4470-be1b-26ed2484bb67', 2, 2, '2004-05-05', '220838', '10102621383', '411131435133', 1, 1, 1, 'https://tavazonex.com', 1);
-- کارگزاری آبان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('6c1bfaa2-3b2b-4e9e-9fe2-06e73e27f8c1', 2, 2, '1996-08-28', '124074', '10101675410', '10101675410', 1, 1, 1, NULL, 1);
-- کارگزاری آبتین استاک
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('fc0de2ed-8590-48e0-9372-75d75d229657', 2, 2, '1900-01-01', '10861466301', '10861466301', '10861466301', 1, 1, 1, 'https://abtinbroker.ir', 1);
-- کارگزاری آپادانا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('97e85bb8-6d5c-4f38-a449-309dd607f870', 2, 2, '1900-01-01', '10861463273', '10861463273', '10861463273', 1, 1, 1, 'http://www.apadanabroker.ir', 1);
-- کارگزاری آتی ساز بازار
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('74e8a4ed-c398-41ff-a596-346f929f14a6', 2, 2, '1900-01-01', '10102040894', '10102040894', '10102040894', 1, 1, 1, 'https://atisazbroker.com', 1);
-- کارگزاری آتیه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('1a384de8-32c6-4d69-b907-3fd543d6a240', 2, 2, '1900-01-01', '10101601511', '10101601511', '10101601511', 1, 1, 1, 'http://www.atieh-broker.ir', 1);
-- کارگزاری آرمان تدبیر نقش جهان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('770c3969-1bf0-4f5e-82a9-4812890a80c8', 2, 2, '2005-10-29', '25776', '10260465367', '411176885774', 1, 1, 1, 'http://www.armanbroker.ir', 1);
-- کارگزاری آرمون بورس
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('ab5edb64-7ea2-4d6c-b691-cd751fc38db3', 2, 2, '1900-01-01', '10101388534', '10101388534', '10101388534', 1, 1, 1, 'https://armoonbourse.co.ir', 1);
-- کارگزاری آفتاب درخشان خاورمیانه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('557d0b42-698a-4115-ae48-cf8bf1576650', 2, 2, '1900-01-01', '10102711784', '10102711784', '10102711784', 1, 1, 1, 'http://www.mesbroker.ir', 1);
-- کارگزاری آگاه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('adb47bff-2ee9-4663-a20f-d49dcceb5d01', 2, 2, '2005-09-08', '6252', '10760338117', '411198944799', 1, 1, 1, 'https://agah.com', 1);
-- کارگزاری آینده نگر خوارزمی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7ec690e0-8765-4d3d-8a1a-d0a21e6eedad', 2, 2, '2006-04-26', '6365', '10460096051', '411134571889', 1, 1, 1, 'http://www.kharazmibroker.ir', 1);
-- کارگزاری اردیبهشت ایرانیان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('eed46a45-9e88-4616-963d-125b6852f7d8', 2, 2, '2005-10-01', '642138', '10260463521', '411171638311', 1, 1, 1, 'https://oibourse.ir', 1);
-- کارگزاری ارگ هومن
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('77dc2582-58ea-45c3-a4e3-127affa3a0b1', 2, 2, '1900-01-01', '10860569396', '10860569396', '10860569396', 1, 1, 1, 'http://www.argbroker.com', 1);
-- کارگزاری اطمینان سهم
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('017211f5-362a-4d90-81dd-22d9fedaa45f', 2, 2, '1900-01-01', '10101751257', '10101751257', '10101751257', 1, 1, 1, NULL, 0);
-- کارگزاری اعتبار تابان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('9f349bdf-770d-4a9f-ba0f-2551faab0b03', 2, 2, '1900-01-01', '14011438469', '14011438469', '14011438469', 1, 1, 1, 'https://etebartaban.ir', 1);
-- کارگزاری اقتصاد بیدار
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('6e6f0fb4-71d1-435c-a1f3-4c4309dd726d', 2, 2, '1900-01-01', '10102658200', '10102658200', '10102658200', 1, 1, 1, 'https://ebidar.com', 1);
-- کارگزاری الوند
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('a15f529f-8ddb-46d9-acd3-6a12795a133f', 2, 2, '1900-01-01', '10101541186', '10101541186', '10101541186', 1, 1, 1, 'https://www.alvandbroker.ir', 1);
-- کارگزاری امید سهم
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('91498df8-fa6c-4c4d-ab4e-c4a49dc05f5e', 2, 2, '1900-01-01', '10720228495', '10720228495', '10720228495', 1, 1, 1, 'http://www.omidsahmco.ir', 1);
-- کارگزاری امین آوید
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('c960388f-0aa6-4259-8c4d-cefe819709b9', 2, 2, '1900-01-01', '10103766525', '10103766525', '10103766525', 1, 1, 1, 'https://aminavid.ir', 1);
-- کارگزاری امین سهم
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('521c2c6e-4dd3-401d-8c8e-fb899e4c0394', 2, 2, '1900-01-01', '10102744567', '10102744567', '10102744567', 1, 1, 1, 'https://aminsahm.com/home', 1);
-- کارگزاری اندیشه و بینش پیشرو
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('cf772558-d755-4542-84e9-6effaf338657', 2, 2, '2000-01-25', '159194', '10102018864', '411331343789', 1, 1, 1, 'https://pishrobroker.ir', 1);
-- کارگزاری اوراق بهادار آسمان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('cf6fe063-c390-44b2-a6c2-01a67247b462', 2, 2, '2022-08-29', '601117', '14011463190', '14011463190', 1, 1, 1, 'https://asemanbroker.ir', 1);
-- کارگزاری اوراق بهادار پاداش
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('4fb84e1a-d612-40eb-a19d-9098066e5631', 2, 2, '1900-01-01', '14012467550', '14012467550', '14012467550', 1, 1, 1, NULL, 1);
-- کارگزاری ایده آل کوشا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f517e207-f312-41d6-a693-e67bd59eb5e5', 2, 2, '1900-01-01', '14012555982', '14012555982', '14012555982', 1, 1, 1, 'https://ideal-brokerage.ir', 1);
-- کارگزاری ایساتیس پویا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f5e48d04-f458-4d96-a993-40e1bc10f454', 2, 2, '1900-01-01', '10860186874', '10860186874', '10860186874', 1, 1, 1, 'https://ipb.ir', 1);
-- کارگزاری ایمن بورس
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('43b256df-6cf0-4d36-9921-969a476b80e4', 2, 2, '2026-02-14', '109871', '10101536021', '411119136976    ', 1, 1, 1, 'http://imen-bourse.ir', 1);
-- کارگزاری بازار سهام
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('2c8ce095-be97-42a8-bd67-e91cf78c7f16', 2, 2, '1900-01-01', '10101479119', '10101479119', '10101479119', 1, 1, 1, 'https://bazarsahambourse.ir', 1);
-- کارگزاری بانک آینده
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('8f2bd82f-9ad0-4404-aaa2-3bd490b0074a', 2, 2, '1900-01-01', '10760335630', '10760335630', '10760335630', 1, 1, 1, 'https://soodayand.ir', 1);
-- کارگزاری بانک اقتصادنوین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('038dab28-182c-42f2-a87c-c116639eef88', 2, 2, '1900-01-01', '10102730837', '10102730837', '10102730837', 1, 1, 1, 'https://enovinbourse.ir', 1);
-- کارگزاری بانک انصار
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('bae6fdf0-190c-46eb-be6e-ec347e2fe0fb', 2, 2, '1900-01-01', '10380399042', '10380399042', '10380399042', 1, 1, 1, 'https://www.ansarbankbroker.ir', 1);
-- کارگزاری بانک پاسارگاد
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('b9a6e69c-fff4-4043-aa4b-a7475a1253f2', 2, 2, '2006-06-11', '6889', '10780103032', '4113-1194-8678', 1, 1, 1, 'http://www.pasargadbroker.ir', 1);
-- کارگزاری بانک تجارت
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('96dfc74e-e772-42b0-afa0-49c5858b64d4', 2, 2, '1900-01-01', '10101441180', '10101441180', '10101441180', 1, 1, 1, 'https://tejaratbankbrk.ir', 1);
-- کارگزاری بانک خاورمیانه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('1d09fabb-e873-4a56-8cbf-a5dbdccc7198', 2, 2, '1996-03-06', '120215', '10101637597', '411119195855', 1, 1, 1, 'http://www.mebbco.ir', 1);
-- کارگزاری بانک دی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('8f344e3b-5c7f-4b24-bb69-bad62d8f5de6', 2, 2, '1900-01-01', '10102075362', '10102075362', '10102075362', 1, 1, 1, 'https://daybroker.ir', 1);
-- کارگزاری بانک رفاه کارگران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('659886ca-5fad-4800-b7be-8acd8199a562', 2, 2, '1900-01-01', '10101477535', '10101477535', '10101477535', 1, 1, 1, 'https://refahbroker.ir', 1);
-- کارگزاری بانک سامان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('d2a93ace-c38b-45cb-96b6-f2c6fc9a936c', 2, 2, '1993-05-23', '97364', '10101414058', '411145699194', 1, 1, 1, 'https://samanbourse.ir', 1);
-- کارگزاری بانک سپه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('95f2312c-36c5-4d7e-b968-29c3d3c5c42b', 2, 2, '1900-01-01', '10101425199', '10101425199', '10101425199', 1, 1, 1, 'https://sepahbroker.ir', 1);
-- کارگزاری بانک صادرات ایران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('d0f6556e-76d8-42a9-8b1b-721a4b3c857c', 2, 2, '1994-01-29', '102578', '10101465218', '411111379751', 1, 1, 1, 'http://www.saderatbourse.ir', 1);
-- کارگزاری بانک صنعت ومعدن
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('b1e93cad-1fef-4a83-a360-209085e7a1f5', 2, 2, '1900-01-01', '10101442691', '10101442691', '10101442691', 1, 1, 1, 'http://www.smbroker.ir', 1);
-- کارگزاری بانک کارآفرین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('aaa85067-7e42-40e2-86e6-7c4ce038794f', 2, 2, '1900-01-01', '10240089809', '10240089809', '10240089809', 1, 1, 1, 'www.karafarinbk.ir', 1);
-- کارگزاری بانک کشاورزی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('dd63da8c-1472-44ce-a4c1-b589384fb55c', 2, 2, '1900-01-01', '10101476080', '10101476080', '10101476080', 1, 1, 1, 'https://www.bkisecurities.com', 1);
-- کارگزاری بانک مسکن
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('48ef6d5c-30c7-43dc-90bd-7f9f4b7a2213', 2, 2, '1900-01-01', '10101402790', '10101402790', '10101402790', 1, 1, 1, 'https://maskanbrokerage.ir', 1);
-- کارگزاری بانک ملت
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('fd35de80-2dc1-4b22-adbd-5ebdcc4b42c7', 2, 2, '1900-01-01', '10101443196', '10101443196', '10101443196', 1, 1, 1, 'http://www.mellatbroker.ir', 1);
-- کارگزاری بانک ملی ایران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('28e8cad9-ceab-40f9-9b5a-e735a6c199cb', 2, 2, '1900-01-01', '10102417115', '10102417115', '10102417115', 1, 1, 1, 'https://bmibourse.ir', 1);
-- کارگزاری باهنر
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('80a2bfec-778e-4532-b9d9-11c45ff724f9', 2, 2, '2003-10-28', '211012', '10102524852', '411119176878', 1, 1, 1, 'https://bahonarbroker.ir', 1);
-- کارگزاری برهان سهند
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('1c839e0f-2aaf-47da-8843-a9cdb5ddfe22', 2, 2, '2005-11-29', '18609', '10200241976', '411166697494', 1, 1, 1, 'http://www.bsbourse.ir', 1);
-- کارگزاری بهمن
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f13c4ab9-9b4a-4028-b3d0-5f6eb81c49f6', 2, 2, '2005-02-27', '622218', '10380384710', '411191916665', 1, 1, 1, 'http://www.bahmanbroker.ir', 1);
-- کارگزاری بهین پویا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('bed78e78-9ef2-4841-b1dd-b1eb0a2c7417', 2, 2, '2006-06-06', '27239', '10260479652', '411171637418', 1, 1, 1, 'http://www.behinpooya.ir', 1);
-- کارگزاری بورس ابراز
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7013343c-0714-4cc2-82e0-3d367be0889d', 2, 2, '1999-03-06', '148738', '10101916009', '10101916009', 1, 1, 1, 'http://ebrazonline.ir', 1);
-- کارگزاری بورس به گزین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('8bcb786a-97ea-453a-a6a1-abf53916e3d3', 2, 2, '1900-01-01', '10101459751', '10101459751', '10101459751', 1, 1, 1, 'http://www.behgozinbroker.ir', 1);
-- کارگزاری بورس بیمه ایران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3a05a0e9-da82-4a4d-8e25-f464265c9e9d', 2, 2, '1996-08-10', '123683', '10101671551', '411145789754', 1, 1, 1, 'https://www.bimeiranbroker.ir', 1);
-- کارگزاری بورسیران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('c945adb7-a1cf-4015-a436-97b7cd700a3f', 2, 2, '1900-01-01', '10101484708', '10101484708', '10101484708', 1, 1, 1, 'http://www.Boursiran.ir', 1);
-- کارگزاری پارس ایده بنیان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('42374bcf-1247-4e9f-841e-b2f9bebae34c', 2, 2, '2008-10-12', '335806', '10103721746', '411338916887', 1, 1, 1, 'http://www.pibbroker.ir', 1);
-- کارگزاری پارس نمودگر
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3f079b04-4014-4b01-ad77-d126833dc947', 2, 2, '1900-01-01', '10101513112', '10101513112', '10101513112', 1, 1, 1, NULL, 0);
-- کارگزاری پارسیان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('cb40582f-3c2d-4494-8cc9-73407c6a0bcd', 2, 2, '1900-01-01', '10380393510', '10380393510', '10380393510', 1, 1, 1, 'www.parsianbroker.ir', 1);
-- کارگزاری پویان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('00373265-6c85-48ca-9238-540a2e745232', 2, 2, '1900-01-01', '14010919910', '14010919910', '14010919910', 1, 1, 1, 'https://www.pouyanbroker.ir', 1);
-- کارگزاری پویش البرز
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('2b3bb7a0-45de-40e9-a270-771d625a1228', 2, 2, '1900-01-01', '14012512164', '14012512164', '14012512164', 1, 1, 1, 'https://epouyesh.ir', 1);
-- کارگزاری پیشگامان بهپرور
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('83524842-20be-4a1e-bd7e-1c74b3e11beb', 2, 2, '1900-01-01', '10102631470', '10102631470', '10102631470', 1, 1, 1, 'http://www.ebehparvar.com', 1);
-- کارگزاری تأمین سرمایه تمدن
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('2dccb6d1-9d38-4d6e-bda3-52daa731795f', 2, 2, '2026-02-03', '10101756777', '10101756777', '10101756777', 1, 1, 1, 'https://tamadonbroker.ir', 1);
-- کارگزاری تامین سرمایه نوین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3e6f9eca-72dc-48b2-96f2-304873c9ef28', 2, 2, '1900-01-01', '10530304306', '10530304306', '10530304306', 1, 1, 1, 'http://nibb.ir', 1);
-- کارگزاری تدبیرگران فردا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('61c719f5-8803-4dea-96a3-07e1a0b6c985', 2, 2, '2026-02-14', '8015', '10861464012', '411117677535', 1, 1, 1, 'https://tadbirbroker.ir', 1);
-- کارگزاری تدبیرگر سرمایه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('14dcc517-7eac-427f-8456-adaa43fb7a4c', 2, 2, '1900-01-01', '10102136872', '10102136872', '10102136872', 1, 1, 1, NULL, 0);
-- کارگزاری توانا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f43d52b4-48bc-4a1e-b94a-ba95522c5379', 2, 2, '1900-01-01', '10102019943', '10102019943', '10102019943', 1, 1, 1, 'https://tavanaco.ir', 1);
-- کارگزاری توسعه اندیشه دانا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('781d7a6e-5e00-47e1-b438-a37f963753b7', 2, 2, '2005-12-19', '25283', '10380407105', '411188341779', 1, 1, 1, 'https://danabc.ir', 1);
-- کارگزاری توسعه سرمایه دنیا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('2636620f-4451-4f40-ace5-6086a3df2954', 2, 2, '1900-01-01', '10102036291', '10102036291', '10102036291', 1, 1, 1, 'http://www.tsd-broker.ir', 1);
-- کارگزاری توسعه سهند
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('89ed152f-5c88-492d-af58-e05155df92ee', 2, 2, '1900-01-01', '10200238049', '10200238049', '10200238049', 1, 1, 1, 'http://www.sahandbroker.com', 1);
-- کارگزاری توسعه فردا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('05f61e02-625c-4a41-8711-0624e19ea674', 2, 2, '1900-01-01', '10101637899', '10101637899', '10101637899', 1, 1, 1, 'https://fardabroker.ir', 1);
-- کارگزاری توسعه کشاورزی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7bffd186-bfe4-42fd-a9c8-773742796645', 2, 2, '1900-01-01', '10102634872', '10102634872', '10102634872', 1, 1, 1, 'https://www.tkbroker.ir', 1);
-- کارگزاری توسعه معاملات خردمند
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('1b4c03af-995c-4c0a-bd04-d165e5a464ff', 2, 2, '1900-01-01', '14013218945', '14013218945', '14013218945', 1, 1, 1, 'https://kheradmandbroker.ir', 1);
-- کارگزاری توسعه معاملات کیان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('5dcfb244-a209-48a5-8913-3791ec8fb457', 2, 2, '1900-01-01', '10101548510', '10101548510', '10101548510', 1, 1, 1, 'https://kiantrader.ir', 1);
-- کارگزاری جهان سهم
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('c7aa59c7-5983-4f10-9402-c45c1867b61c', 2, 2, '1900-01-01', '10101630041', '10101630041', '10101630041', 1, 1, 1, 'http://jsbroker.ir/', 1);
-- کارگزاری حافظ
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('73e3c7c0-4584-4691-8b1a-f1e6e180634f', 2, 2, '1900-01-01', '10102744835', '10102744835', '10102744835', 1, 1, 1, 'http://www.hafezbroker.ir', 1);
-- کارگزاری خبرگان سهام
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('ff503506-ce1a-492b-adea-e3ae587b2ff7', 2, 2, '1900-01-01', '10101553125', '10101553125', '10101553125', 1, 1, 1, 'http://www.khobregan.com', 1);
-- کارگزاری خلیج فارس
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('ca8cd035-8fdf-422a-96e0-e0826be18e08', 2, 2, '1900-01-01', '10101638400', '10101638400', '10101638400', 1, 1, 1, 'https://arianovinbroker.com', 1);
-- کارگزاری دارا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('e4d17127-9ef7-4aad-affb-095a99736f5c', 2, 2, '1900-01-01', '10101509380', '10101509380', '10101509380', 1, 1, 1, 'http://www.darabroker.ir', 1);
-- کارگزاری دانایان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('2b021cf9-eeb5-4f80-bf7c-6f57aec20863', 2, 2, '2005-03-01', '23363', '10102828004', '411188339467', 1, 1, 1, 'https://danayan.broker', 1);
-- کارگزاری دلیران پارس
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('cb0cfe8c-bff6-4338-964a-09a56122f0b9', 2, 2, '1900-01-01', '10102603538', '10102603538', '10102603538', 1, 1, 1, 'https://www.daliranbroker.com', 1);
-- کارگزاری دنیای خبره
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('d2c351ee-faa4-4b3f-939f-00acb451300c', 2, 2, '1900-01-01', '10101514151', '10101514151', '10101514151', 1, 1, 1, 'http://www.dkhobreh.ir', 1);
-- کارگزاری دنیای نوین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3a25426d-7a57-47ea-8be0-8dff934c25d9', 2, 2, '1900-01-01', '10102855894', '10102855894', '10102855894', 1, 1, 1, 'http://dnovinbr.ir', 1);
-- کارگزاری راهبرد سرمایه گذاری ایران سهام
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3e3d01c0-0ffd-4cda-9bee-a1b6d4885e04', 2, 2, '1900-01-01', '10101699412', '10101699412', '10101699412', 1, 1, 1, 'https://www.erahbord.com', 1);
-- کارگزاری راهین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('e3cbc14c-84e7-4a97-9c2d-b58a642b5892', 2, 2, '1900-01-01', '14011404160', '14011404160', '14011404160', 1, 1, 1, 'https://rahinbrokerage.ir', 1);
-- کارگزاری ساردو خاورمیانه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('20b5ba0c-5491-476b-a50d-696aed2c12fa', 2, 2, '2009-07-15', '352302', '10104002107', '411355798483', 1, 1, 1, 'https://mecb.co.ir', 1);
-- کارگزاری ساو آفرین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('38334170-5229-4e98-8d9c-99800d835b03', 2, 2, '1992-03-15', '89164', '10101335524', '411118778311', 1, 1, 1, 'https://www.savbroker.ir', 1);
-- کارگزاری سپهرباستان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('4878bc1d-77fb-42b8-ad73-a11dc39b23c5', 2, 2, '2008-12-06', '336103', '10103768829', '411343374863', 1, 1, 1, 'https://www.sepehrebastan.ir', 1);
-- کارگزاری ستاره جنوب
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('65d8a205-6780-4dd4-90b0-62cce656dc7a', 2, 2, '1900-01-01', '10102558446', '10102558446', '10102558446', 1, 1, 1, 'https://sjbourse.ir', 1);
-- کارگزاری سرمایه گذاری ملی ایران
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('ff3ff380-5b39-4d0b-b654-f0ca2c1b18cf', 2, 2, '1900-01-01', '10102894756', '10102894756', '10102894756', 1, 1, 1, 'http://www.nibi.ir', 1);
-- کارگزاری سرمایه و دانش
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('67a2f20c-7216-4b07-86ab-f753cbf3ff8c', 2, 2, '1900-01-01', '10102731291', '10102731291', '10102731291', 1, 1, 1, 'https://www.sdbroker.ir', 1);
-- کارگزاری سهام بارز
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('9db702e7-4fd6-4a11-b78b-327737a49cf5', 2, 2, '1900-01-01', '10102083025', '10102083025', '10102083025', 1, 1, 1, 'https://barez.ir', 1);
-- کارگزاری سهام پژوهان شایان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('0315c2a9-1a64-4c23-9b4b-d84f14b5e4f7', 2, 2, '1900-01-01', '10101541678', '10101541678', '10101541678', 1, 1, 1, 'https://spshayan.ir', 1);
-- کارگزاری سهام گستران شرق
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7a0d1086-7cfd-4e53-a44e-65f03a84b96e', 2, 2, '1900-01-01', '10380380610', '10380380610', '10380380610', 1, 1, 1, 'https://sgsb.ir', 1);
-- کارگزاری سهم آذین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f419a398-3137-4110-9568-7c2f7b157436', 2, 2, '1900-01-01', '10102000328', '10102000328', '10102000328', 1, 1, 1, 'http://azinbroker.ir', 1);
-- کارگزاری سهم آشنا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('859cfd65-d8ba-46f7-8f4f-fe4dff05eba8', 2, 2, '1900-01-01', '10101628838', '10101628838', '10101628838', 1, 1, 1, 'https://www.abco.ir', 1);
-- کارگزاری سهم یار
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('54f51ec4-08b0-44d8-b75d-260aa5eee742', 2, 2, '1900-01-01', '10101637392', '10101637392', '10101637392', 1, 1, 1, 'http://www.sahmyar.ir', 1);
-- کارگزاری سینا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('9359d70f-32b1-4e04-8703-c9c599fb3baf', 2, 2, '1900-01-01', '236632', '10102775538', '10102775538', 1, 1, 1, 'https://sinabroker.ir', 1);
-- کارگزاری سی ولکس
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('5328a7cb-965e-462e-8548-ccc3ced10e49', 2, 2, '1900-01-01', '10101730772', '10101730772', '10101730772', 1, 1, 1, 'https://seavolex.ir', 1);
-- کارگزاری شاخص سهام
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('1f58c1cf-d98b-40f2-b622-f28104db7b83', 2, 2, '1900-01-01', '10101636315', '10101636315', '10101636315', 1, 1, 1, NULL, 1);
-- کارگزاری شهر
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('290f6590-b6f3-48d4-820c-b73a149dca4e', 2, 2, '1900-01-01', '10104008822', '10104008822', '10104008822', 1, 1, 1, 'http://www.shahrb.ir', 1);
-- کارگزاری صبا تامین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3e64f576-2dbd-4e3e-b780-168840e2e09b', 2, 2, '2026-02-14', '112281', '10101559627', '411111357335', 1, 1, 1, 'http://www.sababroker.ir', 1);
-- کارگزاری صبا جهاد
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('ad2db707-57c2-4911-ab02-7ec331d4695a', 2, 2, '1900-01-01', '10102702966', '10102702966', '10102702966', 1, 1, 1, 'https://sjb.co.ir', 1);
-- کارگزاری فاخر
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('e4be6968-d198-4885-9be7-146b13433e8f', 2, 2, '1900-01-01', '14014379113', '14014379113', '14014379113', 1, 1, 1, NULL, 1);
-- کارگزاری فارابی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('11b43146-cbaa-4131-853a-a8ff07ebce1a', 2, 2, '1994-03-05', '103561', '10101474722', '10101474722', 1, 1, 1, 'http://www.irfarabi.com', 1);
-- کارگزاری فدک
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('d4ca22a7-e093-4e0f-a5d0-ce932bbe8ada', 2, 2, '1993-01-26', '۹۵۱۷۴', '10101392396', '411111311411', 1, 1, 1, 'https://fadakbrokerage.ir', 1);
-- کارگزاری فیروزه آسیا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3ea8e0e6-9f25-4617-9cd1-1c2d1d1d4118', 2, 2, '1900-01-01', '10101612614', '10101612614', '10101612614', 1, 1, 1, 'https://firouzehasia.ir', 1);
-- کارگزاری کارآمد
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('7e641e0d-b3fd-4859-8852-b077749fba77', 2, 2, '1996-09-21', '124620', '10101680726', '411119318456', 1, 1, 1, 'https://www.karamadbrokerage.com', 1);
-- کارگزاری کاریزما
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('0c63c715-f6cf-4b5b-a240-8f5ea3fc80ad', 2, 2, '1994-08-24', '107074', '10101509360', '411137533914', 1, 1, 1, 'https://br.charisma.ir', 1);
-- کارگزاری گنجینه سپهر پارت
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('69cc6ac3-b8fd-41fa-8d78-917d332ceb24', 2, 2, '1900-01-01', '10102605905', '10102605905', '10102605905', 1, 1, 1, 'https://iganjineh.ir', 1);
-- کارگزاری مبین سرمایه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f459b7c9-9390-4f25-ba1e-d3d86ed0a03b', 2, 2, '1900-01-01', '10860222183', '10860222183', '10860222183', 1, 1, 1, 'https://mobinsb.ir', 1);
-- کارگزاری معاملات آرتان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('81aba1e4-72b3-40fb-815f-eefe97365209', 2, 2, '1900-01-01', '14012195586', '14012195586', '14012195586', 1, 1, 1, 'https://artanbroker.ir', 1);
-- کارگزاری مفید
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('c3a1b6ce-3f9b-42ee-9b80-68cea73074fc', 2, 2, '1900-01-01', '10101534680', '10101534680', '10101534680', 1, 1, 1, 'http://www.emofid.com', 1);
-- کارگزاری ملل پویا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('9fdd5416-2939-4ffc-8514-1715a496611e', 2, 2, '1900-01-01', '10530306063', '10530306063', '10530306063', 1, 1, 1, 'https://melalsecurities.ir', 1);
-- کارگزاری مهر آفرین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('49629e82-de18-4cad-ac29-a5830e71f601', 2, 2, '2005-02-20', '241377', '10102821255', '411341417614', 1, 1, 1, 'https://mehrafarin.ir', 1);
-- کارگزاری مهر اقتصاد ایرانیان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f54dd67e-fbaf-4c16-8c4e-3845dbef869d', 2, 2, '2000-05-03', '162514', '10102051355', '411341585647', 1, 1, 1, 'https://meibours.ir', 1);
-- کارگزاری نگاه نوین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('9393de71-b5ed-4175-9dbc-762baf1addea', 2, 2, '1900-01-01', '10102673831', '10102673831', '10102673831', 1, 1, 1, 'https://nnovin.ir', 1);
-- کارگزاری نماد شاهدان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('b915359f-b22c-43f5-b4ff-27ed97defa25', 2, 2, '1900-01-01', '10103999275', '10103999275', '10103999275', 1, 1, 1, 'https://namadbroker.ir', 1);
-- کارگزاری نهایت نگر
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('f6178fca-8bf5-40a7-ab6a-5a4bf41f1850', 2, 2, '1900-01-01', '10102002047', '10102002047', '10102002047', 1, 1, 1, 'https://www.nahayatnegar.com/homePage/brokerage', 1);
-- کارگزاری نواندیشان بازار سرمایه
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('6c57dd97-a7df-456b-9730-bf1edacd81d1', 2, 2, '1900-01-01', '10102733260', '10102733260', '10102733260', 1, 1, 1, NULL, 0);
-- کارگزاری هامرز
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('12b6b94e-a765-4d36-acfc-2e868ed42723', 2, 2, '1900-01-01', '14011449008', '14011449008', '14011449008', 1, 1, 1, 'https://hbc.ir', 1);
-- کارگزاری هوشمند رابین
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('d5b9b295-b460-478f-b04d-959fb9055a54', 2, 2, '1900-01-01', '10102642081', '10102642081', '10102642081', 1, 1, 1, 'https://rabinbroker.ir', 1);
-- کارگزاری و بورس اوراق بهادار رضوی
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('4ff631d2-2cc2-4bad-88a4-4ae74ffdbcc7', 2, 2, '1900-01-01', '10861642000', '10861642000', '10861642000', 1, 1, 1, 'http://www.rbc.ir', 1);
-- کارگزاری ویستا
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('3705b5ea-0ccd-441a-86b9-e4f6c4e7d3f8', 2, 2, '1900-01-01', '14014340223', '14014340223', '14014340223', 1, 1, 1, NULL, 1);
-- کارگزاری یزدان
INSERT INTO dbo.[Company] (Id, CompanyTypeId, IndustryId, RegistrationDate, RegistrationNo, NationalId, EconomicCode, RegistrationCountryId, RegistrationRegionId, RegistrationCityId, Website, IsActive)
VALUES ('06f1b766-a407-4ef5-97da-b7ef8295c9e2', 2, 2, '1900-01-01', '14011836308', '14011836308', '14011836308', 1, 1, 1, 'https://yazdanbr.ir', 1);

PRINT 'Company records inserted successfully.';
PRINT 'Inserting CompanyTranslation records (Persian names)...';

INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7c4fb83f-133c-4470-be1b-26ed2484bb67', 12, N'شرکت کارگزاری توازن بازار');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('6c1bfaa2-3b2b-4e9e-9fe2-06e73e27f8c1', 12, N'کارگزاری آبان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('fc0de2ed-8590-48e0-9372-75d75d229657', 12, N'کارگزاری آبتین استاک');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('97e85bb8-6d5c-4f38-a449-309dd607f870', 12, N'کارگزاری آپادانا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('74e8a4ed-c398-41ff-a596-346f929f14a6', 12, N'کارگزاری آتی ساز بازار');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1a384de8-32c6-4d69-b907-3fd543d6a240', 12, N'کارگزاری آتیه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('770c3969-1bf0-4f5e-82a9-4812890a80c8', 12, N'کارگزاری آرمان تدبیر نقش جهان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('ab5edb64-7ea2-4d6c-b691-cd751fc38db3', 12, N'کارگزاری آرمون بورس');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('557d0b42-698a-4115-ae48-cf8bf1576650', 12, N'کارگزاری آفتاب درخشان خاورمیانه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('adb47bff-2ee9-4663-a20f-d49dcceb5d01', 12, N'کارگزاری آگاه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7ec690e0-8765-4d3d-8a1a-d0a21e6eedad', 12, N'کارگزاری آینده نگر خوارزمی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('eed46a45-9e88-4616-963d-125b6852f7d8', 12, N'کارگزاری اردیبهشت ایرانیان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('77dc2582-58ea-45c3-a4e3-127affa3a0b1', 12, N'کارگزاری ارگ هومن');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('017211f5-362a-4d90-81dd-22d9fedaa45f', 12, N'کارگزاری اطمینان سهم');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('9f349bdf-770d-4a9f-ba0f-2551faab0b03', 12, N'کارگزاری اعتبار تابان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('6e6f0fb4-71d1-435c-a1f3-4c4309dd726d', 12, N'کارگزاری اقتصاد بیدار');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('a15f529f-8ddb-46d9-acd3-6a12795a133f', 12, N'کارگزاری الوند');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('91498df8-fa6c-4c4d-ab4e-c4a49dc05f5e', 12, N'کارگزاری امید سهم');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('c960388f-0aa6-4259-8c4d-cefe819709b9', 12, N'کارگزاری امین آوید');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('521c2c6e-4dd3-401d-8c8e-fb899e4c0394', 12, N'کارگزاری امین سهم');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('cf772558-d755-4542-84e9-6effaf338657', 12, N'کارگزاری اندیشه و بینش پیشرو');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('cf6fe063-c390-44b2-a6c2-01a67247b462', 12, N'کارگزاری اوراق بهادار آسمان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('4fb84e1a-d612-40eb-a19d-9098066e5631', 12, N'کارگزاری اوراق بهادار پاداش');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f517e207-f312-41d6-a693-e67bd59eb5e5', 12, N'کارگزاری ایده آل کوشا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f5e48d04-f458-4d96-a993-40e1bc10f454', 12, N'کارگزاری ایساتیس پویا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('43b256df-6cf0-4d36-9921-969a476b80e4', 12, N'کارگزاری ایمن بورس');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2c8ce095-be97-42a8-bd67-e91cf78c7f16', 12, N'کارگزاری بازار سهام');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('8f2bd82f-9ad0-4404-aaa2-3bd490b0074a', 12, N'کارگزاری بانک آینده');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('038dab28-182c-42f2-a87c-c116639eef88', 12, N'کارگزاری بانک اقتصادنوین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('bae6fdf0-190c-46eb-be6e-ec347e2fe0fb', 12, N'کارگزاری بانک انصار');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('b9a6e69c-fff4-4043-aa4b-a7475a1253f2', 12, N'کارگزاری بانک پاسارگاد');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('96dfc74e-e772-42b0-afa0-49c5858b64d4', 12, N'کارگزاری بانک تجارت');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1d09fabb-e873-4a56-8cbf-a5dbdccc7198', 12, N'کارگزاری بانک خاورمیانه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('8f344e3b-5c7f-4b24-bb69-bad62d8f5de6', 12, N'کارگزاری بانک دی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('659886ca-5fad-4800-b7be-8acd8199a562', 12, N'کارگزاری بانک رفاه کارگران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d2a93ace-c38b-45cb-96b6-f2c6fc9a936c', 12, N'کارگزاری بانک سامان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('95f2312c-36c5-4d7e-b968-29c3d3c5c42b', 12, N'کارگزاری بانک سپه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d0f6556e-76d8-42a9-8b1b-721a4b3c857c', 12, N'کارگزاری بانک صادرات ایران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('b1e93cad-1fef-4a83-a360-209085e7a1f5', 12, N'کارگزاری بانک صنعت ومعدن');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('aaa85067-7e42-40e2-86e6-7c4ce038794f', 12, N'کارگزاری بانک کارآفرین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('dd63da8c-1472-44ce-a4c1-b589384fb55c', 12, N'کارگزاری بانک کشاورزی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('48ef6d5c-30c7-43dc-90bd-7f9f4b7a2213', 12, N'کارگزاری بانک مسکن');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('fd35de80-2dc1-4b22-adbd-5ebdcc4b42c7', 12, N'کارگزاری بانک ملت');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('28e8cad9-ceab-40f9-9b5a-e735a6c199cb', 12, N'کارگزاری بانک ملی ایران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('80a2bfec-778e-4532-b9d9-11c45ff724f9', 12, N'کارگزاری باهنر');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1c839e0f-2aaf-47da-8843-a9cdb5ddfe22', 12, N'کارگزاری برهان سهند');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f13c4ab9-9b4a-4028-b3d0-5f6eb81c49f6', 12, N'کارگزاری بهمن');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('bed78e78-9ef2-4841-b1dd-b1eb0a2c7417', 12, N'کارگزاری بهین پویا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7013343c-0714-4cc2-82e0-3d367be0889d', 12, N'کارگزاری بورس ابراز');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('8bcb786a-97ea-453a-a6a1-abf53916e3d3', 12, N'کارگزاری بورس به گزین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3a05a0e9-da82-4a4d-8e25-f464265c9e9d', 12, N'کارگزاری بورس بیمه ایران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('c945adb7-a1cf-4015-a436-97b7cd700a3f', 12, N'کارگزاری بورسیران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('42374bcf-1247-4e9f-841e-b2f9bebae34c', 12, N'کارگزاری پارس ایده بنیان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3f079b04-4014-4b01-ad77-d126833dc947', 12, N'کارگزاری پارس نمودگر');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('cb40582f-3c2d-4494-8cc9-73407c6a0bcd', 12, N'کارگزاری پارسیان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('00373265-6c85-48ca-9238-540a2e745232', 12, N'کارگزاری پویان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2b3bb7a0-45de-40e9-a270-771d625a1228', 12, N'کارگزاری پویش البرز');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('83524842-20be-4a1e-bd7e-1c74b3e11beb', 12, N'کارگزاری پیشگامان بهپرور');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2dccb6d1-9d38-4d6e-bda3-52daa731795f', 12, N'کارگزاری تأمین سرمایه تمدن');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3e6f9eca-72dc-48b2-96f2-304873c9ef28', 12, N'کارگزاری تامین سرمایه نوین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('61c719f5-8803-4dea-96a3-07e1a0b6c985', 12, N'کارگزاری تدبیرگران فردا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('14dcc517-7eac-427f-8456-adaa43fb7a4c', 12, N'کارگزاری تدبیرگر سرمایه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f43d52b4-48bc-4a1e-b94a-ba95522c5379', 12, N'کارگزاری توانا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('781d7a6e-5e00-47e1-b438-a37f963753b7', 12, N'کارگزاری توسعه اندیشه دانا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2636620f-4451-4f40-ace5-6086a3df2954', 12, N'کارگزاری توسعه سرمایه دنیا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('89ed152f-5c88-492d-af58-e05155df92ee', 12, N'کارگزاری توسعه سهند');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('05f61e02-625c-4a41-8711-0624e19ea674', 12, N'کارگزاری توسعه فردا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7bffd186-bfe4-42fd-a9c8-773742796645', 12, N'کارگزاری توسعه کشاورزی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1b4c03af-995c-4c0a-bd04-d165e5a464ff', 12, N'کارگزاری توسعه معاملات خردمند');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('5dcfb244-a209-48a5-8913-3791ec8fb457', 12, N'کارگزاری توسعه معاملات کیان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('c7aa59c7-5983-4f10-9402-c45c1867b61c', 12, N'کارگزاری جهان سهم');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('73e3c7c0-4584-4691-8b1a-f1e6e180634f', 12, N'کارگزاری حافظ');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('ff503506-ce1a-492b-adea-e3ae587b2ff7', 12, N'کارگزاری خبرگان سهام');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('ca8cd035-8fdf-422a-96e0-e0826be18e08', 12, N'کارگزاری خلیج فارس');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('e4d17127-9ef7-4aad-affb-095a99736f5c', 12, N'کارگزاری دارا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2b021cf9-eeb5-4f80-bf7c-6f57aec20863', 12, N'کارگزاری دانایان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('cb0cfe8c-bff6-4338-964a-09a56122f0b9', 12, N'کارگزاری دلیران پارس');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d2c351ee-faa4-4b3f-939f-00acb451300c', 12, N'کارگزاری دنیای خبره');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3a25426d-7a57-47ea-8be0-8dff934c25d9', 12, N'کارگزاری دنیای نوین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3e3d01c0-0ffd-4cda-9bee-a1b6d4885e04', 12, N'کارگزاری راهبرد سرمایه گذاری ایران سهام');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('e3cbc14c-84e7-4a97-9c2d-b58a642b5892', 12, N'کارگزاری راهین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('20b5ba0c-5491-476b-a50d-696aed2c12fa', 12, N'کارگزاری ساردو خاورمیانه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('38334170-5229-4e98-8d9c-99800d835b03', 12, N'کارگزاری ساو آفرین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('4878bc1d-77fb-42b8-ad73-a11dc39b23c5', 12, N'کارگزاری سپهرباستان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('65d8a205-6780-4dd4-90b0-62cce656dc7a', 12, N'کارگزاری ستاره جنوب');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('ff3ff380-5b39-4d0b-b654-f0ca2c1b18cf', 12, N'کارگزاری سرمایه گذاری ملی ایران');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('67a2f20c-7216-4b07-86ab-f753cbf3ff8c', 12, N'کارگزاری سرمایه و دانش');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('9db702e7-4fd6-4a11-b78b-327737a49cf5', 12, N'کارگزاری سهام بارز');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('0315c2a9-1a64-4c23-9b4b-d84f14b5e4f7', 12, N'کارگزاری سهام پژوهان شایان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7a0d1086-7cfd-4e53-a44e-65f03a84b96e', 12, N'کارگزاری سهام گستران شرق');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f419a398-3137-4110-9568-7c2f7b157436', 12, N'کارگزاری سهم آذین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('859cfd65-d8ba-46f7-8f4f-fe4dff05eba8', 12, N'کارگزاری سهم آشنا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('54f51ec4-08b0-44d8-b75d-260aa5eee742', 12, N'کارگزاری سهم یار');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('9359d70f-32b1-4e04-8703-c9c599fb3baf', 12, N'کارگزاری سینا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('5328a7cb-965e-462e-8548-ccc3ced10e49', 12, N'کارگزاری سی ولکس');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1f58c1cf-d98b-40f2-b622-f28104db7b83', 12, N'کارگزاری شاخص سهام');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('290f6590-b6f3-48d4-820c-b73a149dca4e', 12, N'کارگزاری شهر');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3e64f576-2dbd-4e3e-b780-168840e2e09b', 12, N'کارگزاری صبا تامین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('ad2db707-57c2-4911-ab02-7ec331d4695a', 12, N'کارگزاری صبا جهاد');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('e4be6968-d198-4885-9be7-146b13433e8f', 12, N'کارگزاری فاخر');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('11b43146-cbaa-4131-853a-a8ff07ebce1a', 12, N'کارگزاری فارابی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d4ca22a7-e093-4e0f-a5d0-ce932bbe8ada', 12, N'کارگزاری فدک');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3ea8e0e6-9f25-4617-9cd1-1c2d1d1d4118', 12, N'کارگزاری فیروزه آسیا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7e641e0d-b3fd-4859-8852-b077749fba77', 12, N'کارگزاری کارآمد');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('0c63c715-f6cf-4b5b-a240-8f5ea3fc80ad', 12, N'کارگزاری کاریزما');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('69cc6ac3-b8fd-41fa-8d78-917d332ceb24', 12, N'کارگزاری گنجینه سپهر پارت');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f459b7c9-9390-4f25-ba1e-d3d86ed0a03b', 12, N'کارگزاری مبین سرمایه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('81aba1e4-72b3-40fb-815f-eefe97365209', 12, N'کارگزاری معاملات آرتان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('c3a1b6ce-3f9b-42ee-9b80-68cea73074fc', 12, N'کارگزاری مفید');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('9fdd5416-2939-4ffc-8514-1715a496611e', 12, N'کارگزاری ملل پویا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('49629e82-de18-4cad-ac29-a5830e71f601', 12, N'کارگزاری مهر آفرین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f54dd67e-fbaf-4c16-8c4e-3845dbef869d', 12, N'کارگزاری مهر اقتصاد ایرانیان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('9393de71-b5ed-4175-9dbc-762baf1addea', 12, N'کارگزاری نگاه نوین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('b915359f-b22c-43f5-b4ff-27ed97defa25', 12, N'کارگزاری نماد شاهدان');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f6178fca-8bf5-40a7-ab6a-5a4bf41f1850', 12, N'کارگزاری نهایت نگر');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('6c57dd97-a7df-456b-9730-bf1edacd81d1', 12, N'کارگزاری نواندیشان بازار سرمایه');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('12b6b94e-a765-4d36-acfc-2e868ed42723', 12, N'کارگزاری هامرز');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d5b9b295-b460-478f-b04d-959fb9055a54', 12, N'کارگزاری هوشمند رابین');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('4ff631d2-2cc2-4bad-88a4-4ae74ffdbcc7', 12, N'کارگزاری و بورس اوراق بهادار رضوی');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3705b5ea-0ccd-441a-86b9-e4f6c4e7d3f8', 12, N'کارگزاری ویستا');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('06f1b766-a407-4ef5-97da-b7ef8295c9e2', 12, N'کارگزاری یزدان');

PRINT 'Persian translations inserted.';
PRINT 'Inserting English translations where available...';

INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7c4fb83f-133c-4470-be1b-26ed2484bb67', 10, N'tavazon bazar Brokerage co');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('6c1bfaa2-3b2b-4e9e-9fe2-06e73e27f8c1', 10, N'َAban brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('770c3969-1bf0-4f5e-82a9-4812890a80c8', 10, N' Arman Tadbir Naghsh Jahan Brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('adb47bff-2ee9-4663-a20f-d49dcceb5d01', 10, N'Agah Brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7ec690e0-8765-4d3d-8a1a-d0a21e6eedad', 10, N'Kharazmi Brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('eed46a45-9e88-4616-963d-125b6852f7d8', 10, N'Ordibehesht Iranian Brokerage CO');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('cf772558-d755-4542-84e9-6effaf338657', 10, N'pishro');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('b9a6e69c-fff4-4043-aa4b-a7475a1253f2', 10, N'pasargad');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1d09fabb-e873-4a56-8cbf-a5dbdccc7198', 10, N' middleeast bank brokerage company');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('80a2bfec-778e-4532-b9d9-11c45ff724f9', 10, N'bahonarbroker');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('1c839e0f-2aaf-47da-8843-a9cdb5ddfe22', 10, N'Borhansahand Brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f13c4ab9-9b4a-4028-b3d0-5f6eb81c49f6', 10, N'BAHMAN BROKER');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('bed78e78-9ef2-4841-b1dd-b1eb0a2c7417', 10, N'behinpouya');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3a05a0e9-da82-4a4d-8e25-f464265c9e9d', 10, N'Bourse Bimeh Iran Brokerage');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('61c719f5-8803-4dea-96a3-07e1a0b6c985', 10, N'Tadbirgaranfarda Broker');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('781d7a6e-5e00-47e1-b438-a37f963753b7', 10, N'Toseh Andisheh Dana Broker');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('2b021cf9-eeb5-4f80-bf7c-6f57aec20863', 10, N'Danayan');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('38334170-5229-4e98-8d9c-99800d835b03', 10, N'savafarin');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('4878bc1d-77fb-42b8-ad73-a11dc39b23c5', 10, N'sepehrebastan');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('3e64f576-2dbd-4e3e-b780-168840e2e09b', 10, N'sabatamin broker');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('d4ca22a7-e093-4e0f-a5d0-ce932bbe8ada', 10, N'fadak');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('7e641e0d-b3fd-4859-8852-b077749fba77', 10, N'karamad brokerage ');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('0c63c715-f6cf-4b5b-a240-8f5ea3fc80ad', 10, N'Charisma');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('49629e82-de18-4cad-ac29-a5830e71f601', 10, N'Mehrafarin');
INSERT INTO dbo.[CompanyTranslation] (CompanyId, LanguageId, Name) VALUES ('f54dd67e-fbaf-4c16-8c4e-3845dbef869d', 10, N'Mehr Eghtesad Iranian');

PRINT 'English translations inserted (25 records).';

PRINT '';
PRINT '=== Migration Summary ===';
PRINT 'Companies inserted: 121';
PRINT 'Persian translations: 121';
PRINT 'English translations: 25';
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
