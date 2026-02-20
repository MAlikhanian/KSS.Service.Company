# Comprehensive Company Service Endpoint Tests
# Tests all endpoints in correct order and verifies middleware error handling

$ErrorActionPreference = "Continue"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Company Service - Comprehensive Endpoint Tests" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Authenticate
Write-Host "[1/19] Authenticating..." -ForegroundColor Yellow
try {
    $loginBody = @{ Username = "0082280665"; Password = "ABcd@12345" } | ConvertTo-Json
    $authResponse = Invoke-RestMethod -Uri "http://localhost:8000/Api/User/Login" -Method Post -ContentType "application/json" -Body $loginBody
    $token = $authResponse.token
    Write-Host "✅ Authentication successful`n" -ForegroundColor Green
} catch {
    Write-Host "❌ Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Generate unique values for this test run
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$randomSuffix = Get-Random -Minimum 1000 -Maximum 9999
$uniqueNationalId = "$timestamp$randomSuffix"
$uniqueRegNo = "TEST$timestamp$randomSuffix"
# EconomicCode max length is 20, so limit to 20 chars
$econCodeBase = "$timestamp$randomSuffix"
if ($econCodeBase.Length -gt 20) { $econCodeBase = $econCodeBase.Substring(0, 20) }
$uniqueEconCode = $econCodeBase

Write-Host "Test Data:" -ForegroundColor Gray
Write-Host "  NationalId: $uniqueNationalId" -ForegroundColor Gray
Write-Host "  RegistrationNo: $uniqueRegNo" -ForegroundColor Gray
Write-Host "  EconomicCode: $uniqueEconCode`n" -ForegroundColor Gray

# Step 2: Insert Company
Write-Host "[2/19] Testing Company/AddDto..." -ForegroundColor Yellow
$companyId = [Guid]::NewGuid().ToString()
$companyDto = @{
    Id = $companyId
    CompanyTypeId = 1
    IndustryId = 1
    RegistrationDate = "2024-03-01"
    RegistrationNo = $uniqueRegNo
    NationalId = $uniqueNationalId
    EconomicCode = $uniqueEconCode
    RegistrationCountryId = 1
    RegistrationRegionId = 1
    RegistrationCityId = 1
    TaxId = "TAX$timestamp$randomSuffix"
    FoundedDate = "2020-01-01"
    Website = "https://test$timestamp.com"
    IsActive = $true
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/Company/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($companyDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    $testCompanyId = $response.id
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 3: Update Company
Write-Host "[3/19] Testing Company/UpdateDto..." -ForegroundColor Yellow
$companyUpdateDto = @{
    Id = $testCompanyId
    CompanyTypeId = 1
    IndustryId = 2
    RegistrationDate = "2024-03-01"
    RegistrationNo = $uniqueRegNo
    NationalId = $uniqueNationalId
    EconomicCode = $uniqueEconCode
    RegistrationCountryId = 1
    RegistrationRegionId = 1
    RegistrationCityId = 1
    TaxId = "TAX-UPDATED"
    FoundedDate = "2020-01-01"
    Website = "https://test-updated.com"
    IsActive = $true
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/Company/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($companyUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 4: Insert CompanyTranslation
Write-Host "[4/19] Testing CompanyTranslation/AddDto..." -ForegroundColor Yellow
$translationDto = @{
    CompanyId = $testCompanyId
    LanguageId = 1
    Name = "Test Company EN"
    ShortName = "TC EN"
    Description = "Test Description EN"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyTranslation/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($translationDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 5: Update CompanyTranslation
Write-Host "[5/19] Testing CompanyTranslation/UpdateDto..." -ForegroundColor Yellow
$translationUpdateDto = @{
    CompanyId = $testCompanyId
    LanguageId = 1
    Name = "Test Company EN Updated"
    ShortName = "TC EN UPD"
    Description = "Test Description EN Updated"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/CompanyTranslation/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($translationUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 6: Insert CompanyNameHistory
Write-Host "[6/19] Testing CompanyNameHistory/AddDto..." -ForegroundColor Yellow
$nameHistoryId = [Guid]::NewGuid().ToString()
$nameHistoryDto = @{
    Id = $nameHistoryId
    CompanyId = $testCompanyId
    StartDate = "2024-03-01"
    EndDate = $null
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyNameHistory/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($nameHistoryDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    $testNameHistoryId = $response.id
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 7: Update CompanyNameHistory
Write-Host "[7/19] Testing CompanyNameHistory/UpdateDto..." -ForegroundColor Yellow
$nameHistoryUpdateDto = @{
    Id = $testNameHistoryId
    CompanyId = $testCompanyId
    StartDate = "2024-03-01"
    EndDate = "2024-12-31"
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/CompanyNameHistory/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($nameHistoryUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 8: Insert CompanyNameHistoryTranslation
Write-Host "[8/19] Testing CompanyNameHistoryTranslation/AddDto..." -ForegroundColor Yellow
$nameHistoryTranslationDto = @{
    CompanyNameHistoryId = $testNameHistoryId
    LanguageId = 1
    Name = "Name History EN"
    ShortName = "NH EN"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyNameHistoryTranslation/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($nameHistoryTranslationDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 9: Update CompanyNameHistoryTranslation
Write-Host "[9/19] Testing CompanyNameHistoryTranslation/UpdateDto..." -ForegroundColor Yellow
$nameHistoryTranslationUpdateDto = @{
    CompanyNameHistoryId = $testNameHistoryId
    LanguageId = 1
    Name = "Name History EN Updated"
    ShortName = "NH EN UPD"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/CompanyNameHistoryTranslation/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($nameHistoryTranslationUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 10: Insert Email (for company)
Write-Host "[10/19] Testing Email/AddDto..." -ForegroundColor Yellow
$emailId = [Guid]::NewGuid().ToString()
$emailDto = @{
    Id = $emailId
    CompanyId = $testCompanyId
    LabelId = 1
    EmailAddress = "contact$timestamp@testcompany.com"
    IsPrimary = $true
    IsVerified = $false
    VerifiedAt = $null
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/Email/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($emailDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 11: Insert Phone (for company, E.164 format)
Write-Host "[11/19] Testing Phone/AddDto..." -ForegroundColor Yellow
$phoneId = [Guid]::NewGuid().ToString()
$phoneDto = @{
    Id = $phoneId
    CompanyId = $testCompanyId
    LabelId = 1
    CountryId = 1
    PhoneNumber = "+989123456789"
    IsPrimary = $true
    IsVerified = $false
    VerifiedAt = $null
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/Phone/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($phoneDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 12: Insert Address (for company)
Write-Host "[12/19] Testing Address/AddDto..." -ForegroundColor Yellow
$addressId = [Guid]::NewGuid().ToString()
$addressDto = @{
    Id = $addressId
    CompanyId = $testCompanyId
    LabelId = 1
    CountryId = 1
    RegionId = 1
    CityId = 1
    PostalCode = "1234567890"
    Latitude = $null
    Longitude = $null
    IsPrimary = $true
    IsVerified = $false
    VerifiedAt = $null
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/Address/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($addressDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    $testAddressId = $response.id
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 13: Insert AddressTranslation (for address)
Write-Host "[13/19] Testing AddressTranslation/AddDto..." -ForegroundColor Yellow
$addressTranslationDto = @{
    AddressId = $testAddressId
    LanguageId = 1
    Street1 = "123 Main Street"
    Street2 = "Suite 100"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/AddressTranslation/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($addressTranslationDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 14: Insert CompanyStakeholder
Write-Host "[14/19] Testing CompanyStakeholder/AddDto..." -ForegroundColor Yellow
$stakeholderId = [Guid]::NewGuid().ToString()
$stakeholderDto = @{
    Id = $stakeholderId
    CompanyId = $testCompanyId
    RelatedPartyType = 2
    RelatedPartyId = [Guid]::NewGuid().ToString()
    StakeholderTypeId = 1
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyStakeholder/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($stakeholderDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    $testStakeholderId = $response.id
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 15: Update CompanyStakeholder
Write-Host "[15/19] Testing CompanyStakeholder/UpdateDto..." -ForegroundColor Yellow
$stakeholderUpdateDto = @{
    Id = $testStakeholderId
    CompanyId = $testCompanyId
    RelatedPartyType = 2
    RelatedPartyId = [Guid]::NewGuid().ToString()
    StakeholderTypeId = 2
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/CompanyStakeholder/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($stakeholderUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 16: Insert CompanyStakeholderHistory
Write-Host "[16/19] Testing CompanyStakeholderHistory/AddDto..." -ForegroundColor Yellow
$historyId = [Guid]::NewGuid().ToString()
$historyDto = @{
    Id = $historyId
    CompanyStakeholderId = $testStakeholderId
    OwnershipPercentage = 25.50
    ShareCount = 1000
    BoardRepresentativePersonId = $null
    RegistrationDate = "2024-03-01"
    EffectiveDate = "2024-03-01"
    EndDate = $null
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyStakeholderHistory/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($historyDto))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    $testHistoryId = $response.id
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 17: Update CompanyStakeholderHistory
Write-Host "[17/19] Testing CompanyStakeholderHistory/UpdateDto..." -ForegroundColor Yellow
$historyUpdateDto = @{
    Id = $testHistoryId
    CompanyStakeholderId = $testStakeholderId
    OwnershipPercentage = 30.75
    ShareCount = 1500
    BoardRepresentativePersonId = $null
    RegistrationDate = "2024-03-01"
    EffectiveDate = "2024-03-01"
    EndDate = "2024-12-31"
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/CompanyStakeholderHistory/UpdateDto" -Method Put -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($historyUpdateDto)) -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host $responseBody -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 18: Test CompanyOperation/Insert (Custom Operation)
Write-Host "[18/19] Testing CompanyOperation/Insert..." -ForegroundColor Yellow
$timestamp2 = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$randomSuffix2 = Get-Random -Minimum 1000 -Maximum 9999
$uniqueNationalId2 = "$timestamp2$randomSuffix2"
$uniqueRegNo2 = "TEST$timestamp2$randomSuffix2"
# EconomicCode max length is 20, so limit to 20 chars
$econCodeBase2 = "$timestamp2$randomSuffix2"
if ($econCodeBase2.Length -gt 20) { $econCodeBase2 = $econCodeBase2.Substring(0, 20) }
$uniqueEconCode2 = $econCodeBase2

$jsonBody = '{"Id":"' + [Guid]::NewGuid().ToString() + '","CompanyTypeId":1,"IndustryId":1,"RegistrationDate":"2024-04-01","RegistrationNo":"' + $uniqueRegNo2 + '","NationalId":"' + $uniqueNationalId2 + '","EconomicCode":"' + $uniqueEconCode2 + '","RegistrationCountryId":1,"RegistrationRegionId":1,"RegistrationCityId":1,"TaxId":"TAX' + $timestamp2 + '","FoundedDate":"2021-01-01","Website":"https://test' + $timestamp2 + '.com","IsActive":true,"Translations":[{"LanguageId":1,"Name":"Test Company EN","ShortName":"TC","Description":"Description EN"},{"LanguageId":2,"Name":"Test Company FA","ShortName":"TCFA","Description":"Description FA"}],"NameHistory":{"StartDate":"2024-04-01","EndDate":null,"Translations":[{"LanguageId":1,"Name":"Original Name EN","ShortName":"ON EN"},{"LanguageId":2,"Name":"Original Name FA","ShortName":"ON FA"}]}}'

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8002/Api/CompanyOperation/Insert" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($jsonBody))
    Write-Host "✅ Response:" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
    Write-Host ""
} catch {
    Write-Host "❌ Error:" -ForegroundColor Red
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($errorResponse) {
        Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor Red
    } else {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Write-Host ""
}

# Step 19: Test Duplicate Key Error (Middleware Test)
Write-Host "[19/19] Testing Duplicate Key Error (Middleware Test)..." -ForegroundColor Yellow
Write-Host "Attempting to insert Company with duplicate NationalId..." -ForegroundColor Gray
# EconomicCode max length is 20
$dupEconCode = "DUP$timestamp$randomSuffix"
if ($dupEconCode.Length -gt 20) { $dupEconCode = $dupEconCode.Substring(0, 20) }
$duplicateCompanyDto = @{
    Id = [Guid]::NewGuid().ToString()
    CompanyTypeId = 1
    IndustryId = 1
    RegistrationDate = "2024-03-01"
    RegistrationNo = "DUP$timestamp$randomSuffix"
    NationalId = $uniqueNationalId  # Same as first company!
    EconomicCode = $dupEconCode
    RegistrationCountryId = 1
    RegistrationRegionId = 1
    RegistrationCityId = 1
    TaxId = "TAX-DUPLICATE"
    FoundedDate = "2020-01-01"
    Website = "https://duplicate.com"
    IsActive = $true
    CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
    UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/Company/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($duplicateCompanyDto)) -ErrorAction Stop
    Write-Host "❌ UNEXPECTED: Should have failed with duplicate key error!" -ForegroundColor Red
    Write-Host ""
} catch {
    Write-Host "✅ Expected Error Caught by Middleware:" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        $errorResponse = $responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($errorResponse) {
            Write-Host "Status Code: $($errorResponse.statusCode)" -ForegroundColor Cyan
            Write-Host "Message: $($errorResponse.message)" -ForegroundColor Cyan
            if ($errorResponse.details) {
                Write-Host "Details: $($errorResponse.details)" -ForegroundColor Gray
            }
            Write-Host "`nFull Response:" -ForegroundColor Yellow
            Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor White
        } else {
            Write-Host "Response Body: $responseBody" -ForegroundColor Red
        }
    } else {
        # Try Invoke-WebRequest which provides better error handling
        try {
            $webResponse = Invoke-WebRequest -Uri "http://localhost:8002/Api/Company/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($duplicateCompanyDto)) -ErrorAction SilentlyContinue
        } catch {
            if ($_.Exception.Response) {
                $statusCode = [int]$_.Exception.Response.StatusCode.value__
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                $reader.Close()
                Write-Host "Status Code: $statusCode" -ForegroundColor Cyan
                $errorResponse = $responseBody | ConvertFrom-Json -ErrorAction SilentlyContinue
                if ($errorResponse) {
                    Write-Host "Message: $($errorResponse.message)" -ForegroundColor Cyan
                    if ($errorResponse.details) {
                        Write-Host "Details: $($errorResponse.details)" -ForegroundColor Gray
                    }
                    Write-Host "`nFull Response:" -ForegroundColor Yellow
                    Write-Host ($errorResponse | ConvertTo-Json -Depth 5) -ForegroundColor White
                } else {
                    Write-Host "Response: $responseBody" -ForegroundColor White
                }
            } else {
                Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All Tests Completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
