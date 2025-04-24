
param (
    $certpath,
    $certkeypath,
    $path
)

# Load the certificate from the specified path
$certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPemFile($certpath, $certkeypath)

# Load the content of the file to be signed. Raw is important ;)
$data = Get-Content -Path $path -Raw
$u8Data = [System.Text.Encoding]::UTF8.GetBytes($data)
$dstPath = $path + ".sig"

# Create a new instance of the RSACryptoServiceProvider class and create a signature
$res = $certificate.PrivateKey.SignData($u8Data, [System.Security.Cryptography.HashAlgorithmName]::SHA256, [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
$b64Data = [System.Convert]::ToBase64String($res)

Set-Content -Path $dstPath -Value $b64Data -Encoding UTF8
Write-Host "Signature exported to $dstPath"
