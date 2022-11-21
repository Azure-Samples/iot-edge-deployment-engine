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
    public class IoTEdgeAutomaticDeploymentBuilder : IoTEdgeDeploymentBuilderBase, IIoTEdgeDeploymentBuilder
    {
        private readonly IIoTHubAccessor _ioTHubAccessor;
        private const string AutomaticManifestDirectory = "./DeploymentFiles/AutomaticDeployment";
        private const string RouteKey = "route";

        /// <summary>
        /// ctor
        /// </summary>
        public IoTEdgeAutomaticDeploymentBuilder(IIoTHubAccessor ioTHubAccessor)
        {
            _ioTHubAccessor = ioTHubAccessor;
            //_logger = logger;
        }

        /// <inheritdoc />
        public async Task ApplyDeployments()
        {
            //todo: implement

            throw new NotImplementedException();
        }
    }
}