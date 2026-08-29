<#
    Mints a development JWT the Cart Service will accept, for manual testing only.

    Why this exists: the Identity service has no customer registration endpoint yet, and its
    seeder currently fails (SQL error 2714), so the seeded customer accounts in appsettings
    never make it into the database and /auth/login cannot be used. Once registration or the
    seeder works, log in normally and use that token instead of this script.

    The signing key below is the committed development key from appsettings.json. Tokens minted
    here are worthless against anything but a local dev environment.

    Usage:
        .\mint-test-token.ps1                      # a Customer token, 90 days
        .\mint-test-token.ps1 -Role Admin          # an Admin token (for the 403 test)
        .\mint-test-token.ps1 -Days 7

    Paste the output into the customerToken / adminToken variable of the Postman environment.
#>
param(
    [ValidateSet('Customer', 'Admin', 'Driver')]
    [string]$Role = 'Customer',

    [string]$UserId = '019fd950-77bf-7959-8986-af00456f0e9a',

    [int]$Days = 90
)

$key = 'FlowerIdentityService_Development_JwtSigningKey_ChangeMe_2026_AtLeast32Chars'

function ConvertTo-Base64Url([byte[]]$bytes) {
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$now = [DateTimeOffset]::UtcNow

# ASP.NET Core Identity writes these as the full WS-Federation claim URIs, and every service
# reads them back with RoleClaimType = ClaimTypes.Role / NameClaimType = ClaimTypes.NameIdentifier.
$nameIdClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
$roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

$header = '{"alg":"HS256","typ":"JWT"}'
$payload = "{`"$nameIdClaim`":`"$UserId`",`"$roleClaim`":`"$Role`",`"iss`":`"FlowerIdentityService`",`"aud`":`"FlowerClients`",`"iat`":$($now.ToUnixTimeSeconds()),`"exp`":$($now.AddDays($Days).ToUnixTimeSeconds())}"

$encodedHeader = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($header))
$encodedPayload = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($payload))

$hmac = New-Object System.Security.Cryptography.HMACSHA256
$hmac.Key = [Text.Encoding]::UTF8.GetBytes($key)
$signature = ConvertTo-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$encodedHeader.$encodedPayload")))

Write-Host "Role:    $Role"
Write-Host "User id: $UserId"
Write-Host "Expires: $($now.AddDays($Days).ToString('yyyy-MM-dd'))"
Write-Host ""
"$encodedHeader.$encodedPayload.$signature"
