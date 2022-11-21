using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Accessor;
using IoTEdgeDeploymentEngine.Util;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

//using Microsoft.Extensions.Logging;


namespace IoTEdgeDeploymentEngine
{
    /// <summary>
    /// Layered deployment builder that provides methods to create/apply layered deployments
    /// </summary>
    public class IoTEdgeLayeredDeploymentBuilder : IoTEdgeDeploymentBuilderBase, IIoTEdgeDeploymentBuilder
    {
        private const string LayeredManifestDirectory = "./DeploymentFiles/LayeredDeployment";
        private readonly IIoTHubAccessor _ioTHubAccessor;

        /// <summary>
        /// ctor
        /// </summary>
        public IoTEdgeLayeredDeploymentBuilder(IIoTHubAccessor ioTHubAccessor)
        {
            _ioTHubAccessor = ioTHubAccessor;
            //_logger = logger;
        }

        /// <inheritdoc />
        public async Task ApplyDeployments()
        {
            var configurations = await ReadAllFiles(LayeredManifestDirectory);

            var deviceGroups = configurations.GroupBy(c => c.TargetCondition);
            foreach (var deviceGroup in deviceGroups)
            {
                var modulesSpec = GetEdgeAgentModulesSpec(deviceGroup);
                var edgeAgentDesiredProperties = new EdgeAgentDesiredProperties()
                {
                    SystemModuleVersion = "1.3",
                    RegistryCredentials = GetEdgeAgentSettings(deviceGroup).ToList(),
                    EdgeModuleSpecifications = modulesSpec
                };

                EdgeHubDesiredProperties edgeHubConfig = new EdgeHubDesiredProperties()
                {
                    Routes = GetEdgeHubRoutes(deviceGroup)
                };

                var configurationContent = new ConfigurationContent()
                    .SetEdgeHub(edgeHubConfig)
                    .SetEdgeAgent(edgeAgentDesiredProperties);

                var outerModules = deviceGroup
                    .SelectMany(c => c.Content.ModulesContent.Where(m => m.Key != "$edgeAgent" && m.Key != "$edgeHub"))
                    .Distinct(new KeyValuePairDictionaryEqualComparer());

                foreach (var outerModule in outerModules)
                {
                    if (!outerModule.Value.ContainsKey("properties.desired"))
                        continue;

                    configurationContent.SetModuleDesiredProperty(new ModuleSpecificationDesiredProperties()
                    {
                        Name = outerModule.Key,
                        DesiredProperties = outerModule.Value["properties.desired"],
                    });
                }

                var deviceIds = await _ioTHubAccessor.GetDeviceIdsByCondition(deviceGroup.Key);
                foreach (var device in deviceIds)
                {
                    await _ioTHubAccessor.ApplyDeploymentPerDevice(device, configurationContent);
                }
            }
        }
    }
}