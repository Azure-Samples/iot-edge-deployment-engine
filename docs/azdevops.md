# Azure DevOps Pipelines: setting up a pipeline with IoTEdgeDeployment Engine to trigger deployments

## Contents

- [Introduction](#introduction)
- [Step-by-step setting up the pipeline](#step-by-step)

## Introduction

This project contains a .NET Console application that will apply all deployment to any device matching the manifest files. To automate the merging and applying of manifests through a CI/CD pipeline, you can use the console app as one of the steps in this process. 
This guide shows how you could do this with Azure DevOps. 

## Pre-requisites

- Azure DevOps project
- Azure CLI
- PowerShell
- Required resources in Azure: by having run the developer setup, or by having the following resources available:
    - Azure IoT Hub with edge devices provisioned
    - Azure Key Vault for storing secrets used by the pipeline: optional, you could replace this with usage of variables (groups) in Azure DevOps.

## Step-by-step

This setup assumes the `.json` manifest files required for Automatic and Layered deployments are stored within the code repo, following the concept of GitOps where manifests for Kubernetes clusters are also checked into the repo. 
You have other options by generating the files within your pipelines which will slightly change the steps defined below. Take the below as an example of how this could work but adapt to best fit yours needs.

To create a set of devices in your IoT Hub for testing, and corresponding layered manifests for testing you can run the PowerShell script `deployment/helpers/createDevices.ps1`.

1. First create a base layered deployment `.json` file. To have one file apply to a single device you can use the following `targetCondition`:
  ```json
  "targetCondition": "deviceId = 'TO_REPLACE_DEVICE_ID'",

  ```
2. Leave the `TO_REPLACE_DEVICE_ID` as is, since this script will replace this in the file contents when running the script. A sample [`baselayersample.json`](baselayersample.json) file can be found in this folder.
3. Run the device and file generation script in PowerSheell:

  ```powershell
  .\createDevices.ps1 -iotHubName <iothubname> -numberOfDevices 2 -identityPrefix "<devicenameprefix>" -sourceLayeredManifestFile "<full_path_to_base_layer.json>" -destinationLayeredManifestDirectory "C:\mydocuments\source\stuff\manifests\LayeredDeployment\"
  ```

You can then commit the generated files to your code repo. Here's an example of a base repo that is required for the pipeline to run:

```
─manifests
    ├───AutomaticDeployment
    │       automaticbase.json
    └───LayeredDeployment
            edgedevice-1.json
            edgedevice-2.json
            edgedevice-3.json
            edgedevice-4.json
            edgedevice-5.json
            edgedevice-6.json
            edgedevice-7.json
            edgedevice-8.json
            edgedevice-9.json
            layeredsource.json
```

### Prepare variable and secrets in Azure Key Vault.

Add the following variables to the Azure Key Vault you will be using for storing secrets used by the pipeline:
  - `GITPAT` = GitHub personal access token
  - `GITUSER` = a GitHub repo with access to this private repo
  - `IOTHUBHOSTNAME` = IoT Hub hostname in the form of `xxx.azure-devices.net`
  - `KEYVAULTURI` = Key Vault URI including `https://` prefix. This could be another Key Vault account than the one you are adding your secrets to, as it will be used for replacing secrets in manifest files. This is optional and only needed if you are replacing secret placeholders in the manifest `json` files.

### Assigning RBAC permissions for the Azure DevOps Service Principal

The pipeline will execute in Azure under a Service Principal account that will require access to IoT Hub (3 roles) and Azure Key Vault Secrets Officer.

1. In your Azure DevOps project, add or identify an [Azure Service Connection](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops&tabs=yaml) setup to connect to your Azure chosen subscription. 
2. Identify the Service Principal object used for this connection. Normally you can click the **Manage Service Principal** link to take you directly to Azure Active Directory.
3. Copy the AppId of this Service Principal.
4. Execute the Role assignment script below in a PowerShell terminal:

```powershell
$principalId = <AppId of the service principal>
$keyVaultName = <Key Vault resource name>
$iotHubName = <IoT Hub resource name>
$subscriptionId - <Subscription ID>

az role assignment create --assignee $principalId `
    --role "Key Vault Secrets Officer" `
    --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.KeyVault/vaults/$keyVaultName" -o none

az role assignment create --assignee $principalId `
    --role "IoT Hub Data Contributor" `
    --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Devices/IotHubs/$iotHubName" -o none

az role assignment create --assignee $principalId `
    --role "IoT Hub Registry Contributor" `
    --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Devices/IotHubs/$iotHubName" -o none

az role assignment create --assignee $principalId `
    --role "IoT Hub Twin Contributor" `
    --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Devices/IotHubs/$iotHubName" -o none
```

### Setting up the Azure Pipeline

1. Ensure you have an Azure DevOps project or create a new one.
2. Temporary: if this repo is private, create a GitHub [Private Access Token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) so this repo can be cloned in the Azure DevOps pipeline.
3. Create or ensure you have an Azure Service Connection. This will be used to authenticate to the Azure IoT Hub and Key Vault resources.
4. Create a new pipeline and enter the following YAML, replacing the values in `<>` with your specific values.

```yaml
trigger:
- main

pool:
  vmImage: ubuntu-latest

variables:
- name: buildConfiguration
  value: 'Release'
- name: ROOT_MANIFESTS_FOLDER
  value: $(Build.SourcesDirectory)/manifests

steps:

- task: AzureKeyVault@2
  inputs:
    azureSubscription: '<REPLACE: YOUR AZURE SERVICECONNECTION>'
    KeyVaultName: '<REPLACE: NAME OF THE KEY VAULT>'
    SecretsFilter: 'GITPAT,GITURI,GITUSER,IOTHUBHOSTNAME,KEYVAULTURI'
    RunAsPreJob: true


- task: CmdLine@2
  inputs:
    script: |
      echo 'Cloning engine repo'
      git clone https://$(GITUSER):$(GITPAT)@github.com/Bindsi/IoTEdgeDeploymentService.git --progress --branch main --single-branch --depth=1 $(Build.SourcesDirectory)/engine

- task: CmdLine@2
  inputs:
    script: |
      echo "##vso[task.setvariable variable=IOTHUB_HOSTNAME]$(IOTHUBHOSTNAME)"
      echo "##vso[task.setvariable variable=KEYVAULT_URI]$(KEYVAULTURI)"

- task: AzureCLI@2
  inputs:
    azureSubscription: '<REPLACE: YOUR AZURE SERVICECONNECTION>'
    scriptLocation: 'inlineScript'
    scriptType: bash
    inlineScript: 'dotnet run --project "$(Build.SourcesDirectory)/engine/src/IoTEdgeDeploymentTester/IoTEdgeDeploymentTester.csproj" --configuration Debug'

```
5. Ensure the path to the `manifests` defined by the variable `ROOT_MANIFESTS_FOLDER` is correct in the pipeline. In the current care the Azure DevOps project only has one repo. If you have multiple you will need to correctly address it, see [Build variables, specifically `Build.SourcesDirectory`](https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml#build-variables-devops-services).
5. Run the pipeline and validate each step executed successfully.

## Clean-up resources

Deleting the Azure Pipeline will disable IoT Edge deployments using this pipeline. Cleaning up the directory with manifests will also ensure nothing gets applied to IoT Edge devices in the selected IoT Hub. 
For deleting Azure resources created with the developer setup please see the readme file in the root of this repo.