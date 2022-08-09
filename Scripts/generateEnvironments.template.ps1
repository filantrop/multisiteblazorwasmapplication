
# If script fails because of execution policy
# Open powershell in adminmode (short Windows+X) 
# Then run the following 'Set-ExecutionPolicy RemoteSigned'
# You also have to unblock rules in firewall to prevent changes to hostsfile

Set-Location -Path $PSScriptRoot

$configPath = "..\config\config.template.json"
$runGenerator = Resolve-Path -Path "..\config\RunGenerators\bin\Debug\net6.0\RunGenerators.exe"
$stdOutPath = "..\GitIgnored\"


$stdOutPathResolved = Resolve-Path -Path $stdOutPath
$stdOutPathResolved = $stdOutPath+"log.txt"


if(!(Test-Path -Path $stdOutPath )){
   New-Item -ItemType directory -Path $stdOutPath
}



start-process `
-FilePath  $runGenerator `
-ArgumentList "--configpath  $configPath --stdoutpath $stdOutPathResolved -p -aeloch" `
-verb runas


