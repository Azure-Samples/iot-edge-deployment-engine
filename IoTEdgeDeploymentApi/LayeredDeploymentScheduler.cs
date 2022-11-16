using System;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace IoTEdgeDeploymentApi
{
    public class LayeredDeploymentScheduler
    {
        private readonly IoTEdgeLayeredDeploymentBuilder _ioTEdgeLayeredDeploymentBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ioTEdgeLayeredDeploymentBuilder">IoTEdgeLayeredDeploymentBuilder instance per DI</param>
        public LayeredDeploymentScheduler(IoTEdgeLayeredDeploymentBuilder ioTEdgeLayeredDeploymentBuilder)
        {
            _ioTEdgeLayeredDeploymentBuilder = ioTEdgeLayeredDeploymentBuilder;
        }
        
        [FunctionName("LayeredDeploymentScheduler")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer)
        {
            await _ioTEdgeLayeredDeploymentBuilder.ApplyDeployments();
        }
    }
}
