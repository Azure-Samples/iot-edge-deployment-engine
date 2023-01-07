param (
    $resourceGroupName,
    $location = "West Europe",
    $resourceName,
    $tenantId,
    $appId
)

$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName
if ($null -eq $resourceGroup) {
    New-AzResourceGroup -Name $resourceGroupName -Location $location
}

New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile ./azuredeploy.json -resourceName $resourceName -tenantId $tenantId -appId $appId