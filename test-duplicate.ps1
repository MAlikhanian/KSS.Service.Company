$loginBody = @{ Username = "0082280665"; Password = "ABcd@12345" } | ConvertTo-Json
$authResponse = Invoke-RestMethod -Uri "http://localhost:8000/Api/User/Login" -Method Post -ContentType "application/json" -Body $loginBody
$token = $authResponse.token

$dupBody = '{"Id":"' + [Guid]::NewGuid().ToString() + '","CompanyTypeId":1,"IndustryId":1,"RegistrationDate":"2024-03-01","RegistrationNo":"DUPTEST999","NationalId":"17715876447341","EconomicCode":"DUPTEST999","RegistrationCountryId":1,"RegistrationRegionId":1,"RegistrationCityId":1,"TaxId":"TAX-DUPTEST","FoundedDate":"2020-01-01","Website":"https://dup.com","IsActive":true,"CreatedAt":"2026-01-01T00:00:00Z","UpdatedAt":"2026-01-01T00:00:00Z"}'

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8002/Api/Company/AddDto" -Method Post -ContentType "application/json; charset=utf-8" -Headers @{ "Authorization" = "Bearer $token" } -Body ([System.Text.Encoding]::UTF8.GetBytes($dupBody)) -UseBasicParsing -ErrorAction Stop
    Write-Host "UNEXPECTED SUCCESS"
} catch {
    $statusCode = [int]$_.Exception.Response.StatusCode
    Write-Host "HTTP Status: $statusCode"
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $body = $reader.ReadToEnd()
    $reader.Close()
    Write-Host "Response Body: $body"
}
