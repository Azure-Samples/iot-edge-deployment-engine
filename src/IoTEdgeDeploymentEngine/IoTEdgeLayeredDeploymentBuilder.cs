using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Accessor;
using IoTEdgeDeploymentEngine.Config;
using IoTEdgeDeploymentEngine.Util;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

//using Microsoft.Extensions.Logging;


namespace IoTEdgeDeploymentEngine
{
    /// <summary>
    /// Layered deployment builder that provides methods to create/apply layered deployments
    /// </summary>
    public class IoTEdgeLayeredDeploymentBuilder : IoTEdgeDeploymentBuilderBase
    {
        /// <summary>
        /// ctor
        /// </summary>
        public IoTEdgeLayeredDeploymentBuilder(IIoTHubAccessor ioTHubAccessor, ManifestConfigLayered manifestConfig) : base(ioTHubAccessor, manifestConfig)
        {
        }

        // /// <inheritdoc />
        // protected override string ManifestDirectory => "./DeploymentFiles/LayeredDeployment"; //todo: add to configuration parameter

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<string, object>> GetEdgeAgentModules(IGrouping<string, Configuration> deviceGroup)
        {
            return deviceGroup.SelectMany(c => c.Content.ModulesContent["$edgeAgent"])
                .Distinct(new KeyValuePairEqualComparer());
        }

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<string, IDictionary<string, object>>> GetOuterModules(IGrouping<string, Configuration> deviceGroup)
        {
            return deviceGroup
                .SelectMany(c => c.Content.ModulesContent.Where(m => m.Key != "$edgeAgent" && m.Key != "$edgeHub"))
                .Distinct(new KeyValuePairDictionaryEqualComparer());
        }
    }
}