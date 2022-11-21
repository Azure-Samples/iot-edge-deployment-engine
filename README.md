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

### LayeredDeployment

Provides an endpoint to submit a new layered deployment manifest to be stored.

### AutomaticDeployment
Provides an endpoint to submit a new automatic deployment manifest to be stored.

### LayeredDeploymentScheduler

Executes layered deployment on a timer-based way (default setup: 12:00am)

### AutomaticDeploymentScheduler

Executes layered deployment on a timer-based way (default setup: 12:00am)
