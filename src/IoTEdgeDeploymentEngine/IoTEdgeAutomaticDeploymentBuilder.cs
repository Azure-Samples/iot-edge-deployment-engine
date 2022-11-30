using System;
using System.Collections.Generic;
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
    /// Automatic deployment builder that provides methods to create/apply automatic deployments
    /// </summary>
    public class IoTEdgeAutomaticDeploymentBuilder : IoTEdgeDeploymentBuilderBase
    {
        /// <inheritdoc />
        protected override string ManifestDirectory => "./DeploymentFiles/AutomaticDeployment"; //todo: add to configuration parameter

        /// <summary>
        /// ctor
        /// </summary>
        public IoTEdgeAutomaticDeploymentBuilder(IIoTHubAccessor ioTHubAccessor) : base(ioTHubAccessor)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<string, object>> GetEdgeAgentModules(IGrouping<string, Configuration> deviceGroup)
        {
            return deviceGroup.OrderByDescending(d => d.Priority).Take(1)
                .SelectMany(c => c.Content.ModulesContent["$edgeAgent"])
                .Distinct(new KeyValuePairEqualComparer());
        }
        
        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<string, IDictionary<string, object>>> GetOuterModules(IGrouping<string, Configuration> deviceGroup)
        {
            return deviceGroup.OrderByDescending(d => d.Priority).Take(1)
                .SelectMany(c => c.Content.ModulesContent.Where(m => m.Key != "$edgeAgent" && m.Key != "$edgeHub"))
                .Distinct(new KeyValuePairDictionaryEqualComparer());
        }

    }
}