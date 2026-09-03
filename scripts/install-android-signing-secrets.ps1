param(
    [Parameter(Mandatory = $true)]
    [string]$KeystorePath,

    [string]$KeyAlias,

    [string]$Repo = "adaPlu/ClickDungeon2"
)

$ErrorActionPreference = "Stop"

$expectedFingerprint = "D8:11:9B:59:52:1A:7F:06:94:90:7C:00:BF:D1:F8:AA:9E:85:46:75:83:27:C9:15:95:0B:4A:E1:E7:14:36:64"

function Read-SecretText([string]$Prompt) {
    $secure = Read-Host -AsSecureString $Prompt
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        if ($bstr -ne [IntPtr]::Zero) {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
        }
    }
}

function Require-Command([string]$Name) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        throw "$Name is required but was not found on PATH."
    }
    return $command.Source
}

function Set-GitHubSecret([string]$Name, [string]$Value) {
    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name cannot be empty."
    }
    $Value | gh secret set $Name --repo $Repo --app actions
}

function Normalize-Fingerprint([string]$Fingerprint) {
    $compact = ($Fingerprint -replace "[^0-9A-Fa-f]", "").ToUpperInvariant()
    if ($compact.Length -ne 64) {
        throw "Invalid SHA-256 fingerprint: $Fingerprint"
    }
    $parts = for ($index = 0; $index -lt $compact.Length; $index += 2) {
        $compact.Substring($index, 2)
    }
    return $parts -join ":"
}

$resolvedKeystore = Resolve-Path -LiteralPath $KeystorePath
$keytool = Require-Command "keytool"
Require-Command "gh" | Out-Null

if ([string]::IsNullOrWhiteSpace($KeyAlias)) {
    $KeyAlias = Read-Host "Android key alias"
}
$storePassword = Read-SecretText "Android keystore password"
$keyPassword = Read-SecretText "Android key password"

$certOutput = & $keytool -list -v -keystore $resolvedKeystore.Path -storepass $storePassword -alias $KeyAlias 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "keytool could not read the requested keystore alias."
}

$actualFingerprint = ($certOutput | Select-String "SHA256:" | Select-Object -First 1).ToString().Split("SHA256:")[1].Trim()
$actualFingerprint = Normalize-Fingerprint $actualFingerprint
if ($actualFingerprint -ne $expectedFingerprint) {
    throw "Keystore alias fingerprint mismatch. Expected $expectedFingerprint but found $actualFingerprint."
}

$keystoreBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($resolvedKeystore.Path))

Set-GitHubSecret "ANDROID_KEYSTORE_BASE64" $keystoreBase64
Set-GitHubSecret "ANDROID_KEYSTORE_PASSWORD" $storePassword
Set-GitHubSecret "ANDROID_KEY_ALIAS" $KeyAlias
Set-GitHubSecret "ANDROID_KEY_PASSWORD" $keyPassword

Write-Host "Android signing secrets installed for $Repo."
Write-Host "Verified upload certificate SHA-256: $actualFingerprint"
