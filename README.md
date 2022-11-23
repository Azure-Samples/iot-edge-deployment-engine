# At-scale Deployment

Azure IoT Edge provides two ways of [deployments](https://learn.microsoft.com/en-us/azure/iot-edge/module-deployment-monitoring?view=iotedge-1.4). 
On the hand you can create a deployment manifest to deploy modules and apply it to one single device.
On the other hand you can create a deployment manifest with a tag based filter and modules get applied to the registered devices that matches the defined condition.
The latter one contains automatic deployments where the deployments of the higher priority are only applied to the devices with the same tag(s) 
and layered deployment where modules and routes are consolidated between different deployment definitions for the same devices based by higher priority.

Industries make use of at-scale deployments to define new sets of modules for devices categorized by different keys, e.g. country, region, plant, build etc.
Due to cost savings and to avoid a blown-up cloud environment the same IoTHub instances are used over several departments.
Hence, the customers have the requirement to define a huge amount of at-scale deployments, particularly if they want to be downwards compatible.
Currently, there is still a limit of 100 at-scale deployments you can specify for the IoTHub instance in Azure Portal or via IoTHub Sdk.
So, this leads to the need to build an own deployment engine without limitation with a kind of more flexibility.

# Solution

## File-based IoTEdgeDeploymentEngine

### Layered Deployment

Consolidates deployment manifests per device tag and target condition and creates a merged version including the determined modules and routes to be applied on the devices.
Deployment manifest are stored on the file system in the default schema.

### Automatic Deployment

Applies the latest relevant deployment manifest per device tag based on the highest priority setting to the devices.
As well as for layered deployments the configurations are stored on the file system

## Azure Functions IoTEdgeDeploymentApi

### Overview

Provides API and scheduler functionalities to manage engine.
Swagger UI is fully supported and can be opened via the following [URL][def].

### LayeredDeployment

Provides the following endpoints:

- submit a new layered deployment manifest to be stored
- retrieve deployment manifest file content by a specified file path

### AutomaticDeployment

Provides the following endpoints:

- submit a new automatic deployment manifest to be stored
- retrieve deployment manifest file content by a specified file path

### LayeredDeploymentScheduler

Executes layered deployment on a timer-based way (default setup: 12:00am)

### AutomaticDeploymentScheduler

Executes layered deployment on a timer-based way (default setup: 12:00am)

### How-to

Just paste the IoTHub connection string into the local.settings.json.
Make sure that your access policy includes "Registry Read|Write" permissions (you can use iothubowner).

## Console application IoTEdgeDeploymentTester

### Overview

A simple app that can test the engine.

### How-to

1. Just paste the IoTHub connection string into the console application properties/run configuration as first argument.
Make sure that your access policy includes "Registry Read|Write" permissions (you can use iothubowner).
2. Include additional DI registration and methods calls of your choice into the Program.cs.

## Deployment

1. use the GitHub Actions [workflow file][def2] and set it up in your fork
2. just add the following [secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets#creating-encrypted-secrets-for-a-repository) in your repository settings:

- AZURE_CREDENTIALS --> store the json by following the [instructions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions?tabs=userlevel#generate-deployment-credentials) to obtain your subscription credentials
- AZURE_SUBSCRIPTION --> Azure Subscription id
- AZURE_RG --> Azure Resource Group name
- IOTHUB_CONSTRING --> Azure IoTHub connection string
- STORAGEACCOUNT_NAME --> Azure Storage Account name
- APPINSIGHTS_NAME --> Azure Application Insights name
- HOSTINGPLAN_NAME --> Azure App Service Plan name
- AZUREFUNC_NAME --> Azure Functions name

[def]: http://localhost:7071/api/swagger/ui
[def2]: /.github/workflows/CD_Infra.yml