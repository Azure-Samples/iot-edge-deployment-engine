param (
    $tenantName,
    $spName = "IoTEdgeDeploymentEngine"
)

# Check if App Registration exists
$appReg = Get-AzADApplication -DisplayName $spName
if ($null -ne $appReg) {
    return;
}

# Create App Registration
$uris = @("https://$spName.azurewebsites.net/.auth/login/aad/callback", `
        "https://localhost:7071/swagger/oauth2-redirect.html", `
        "http://localhost:7071/swagger/oauth2-redirect.html", `
        "https://$spName.azurewebsites.net/swagger/oauth2-redirect.html")
$newAppReg = New-AzADApplication -DisplayName $spName -ReplyUrls $uris

# Update App Registration with additional settings
$newAppReg.Web.ImplicitGrantSetting.EnableAccessTokenIssuance = $true
$newAppReg.Web.ImplicitGrantSetting.EnableIdTokenIssuance = $true
$appId = $newAppReg.AppId
$identifierUri = "https://$appId.$tenantName"

Update-AzADApplication -ObjectId $newAppReg.Id -Web $newAppReg.Web -IdentifierUri $identifierUri

# Create Service Principal and link it to newly created App Registration
New-AzADServicePrincipal -ApplicationId $appId