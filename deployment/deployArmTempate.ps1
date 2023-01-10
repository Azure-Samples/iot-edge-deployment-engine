param (
    $resourceGroupName,
    $location = "West Europe",
    $resourceName,
    $tenantId,
    $appId
)

$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if ($null -eq $resourceGroup) {
    New-AzResourceGroup -Name $resourceGroupName -Location $location
    Write-Host "Created new resource group $resourceGroupName"
}

Write-Host "Deploying ARM template..."
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile ./azuredeploy.json -resourceName $resourceName -tenantId $tenantId -appId $appId
Write-Host "Finished."