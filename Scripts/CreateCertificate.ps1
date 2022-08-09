#$mypath = $PSScriptRoot
param ($domains, $password, $certspath)


$domainSplitted = $domains.Split(",")
#$domain = "yourdomain.test"
#$password = "mypassword"
#$filepathDir = "$PSScriptRoot/certs/$domain"

foreach($domain in $domainSplitted)
{
	$filepathDir = "$certspath/$domain"

	write-output "filepathDir: $filepathDir"

	mkdir $filepathDir
	Set-Content "$filepathDir/password.txt" $password
	$filepath = "$filepathDir/$domain.pfx"
	$certFilepath = "$filepathDir/$domain.cer"

	$selfSignedRootCA = New-SelfSignedCertificate -Subject *.$domain -DnsName $domain, *.$domain -notafter (Get-Date).AddMonths(6) -CertStoreLocation Cert:\LocalMachine\My\ -KeyExportPolicy Exportable -KeyUsage CertSign,CRLSign,DigitalSignature -KeySpec KeyExchange -KeyLength 2048 -KeyUsageProperty All -KeyAlgorithm 'RSA' -HashAlgorithm 'SHA256' -Provider 'Microsoft Enhanced RSA and AES Cryptographic Provider'
	$CertPassword = ConvertTo-SecureString -String $password -Force -AsPlainText
	$selfSignedRootCA | Export-PfxCertificate -FilePath $filepath -Password $CertPassword -verbose

	Import-PfxCertificate -FilePath $filepath -Password $CertPassword -CertStoreLocation Cert:\LocalMachine\Root


	openssl pkcs12 -in $filepath -clcerts -nokeys -out $filepathDir/$domain.crt  -passin pass:$password -passout pass:$password
	openssl pkcs12 -in $filepath -nodes -nocerts -out $filepathDir/$domain.key  -passin pass:$password -passout pass:$password

	openssl pkcs12 -in $filepath -out $filepathDir/$domain.txt -nodes -passin pass:$password
}