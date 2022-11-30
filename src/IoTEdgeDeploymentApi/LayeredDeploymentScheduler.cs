using System;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace IoTEdgeDeploymentApi
{
    /// <summary>
    /// Scheduler function to apply deployments.
    /// </summary>
    public class LayeredDeploymentScheduler
    {
        private readonly IIoTEdgeDeploymentBuilder _ioTEdgeLayeredDeploymentBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ioTEdgeLayeredDeploymentBuilder">IoTEdgeLayeredDeploymentBuilder instance per DI</param>
        public LayeredDeploymentScheduler(IoTEdgeLayeredDeploymentBuilder ioTEdgeLayeredDeploymentBuilder)
        {
            _ioTEdgeLayeredDeploymentBuilder = ioTEdgeLayeredDeploymentBuilder;
        }
        
        /// <summary>
        /// Timer based function that executes deployments every day at 12:00am
        /// </summary>
        /// <param name="myTimer">Timer object</param>
        [FunctionName("LayeredDeploymentScheduler")]
        public async Task Run([TimerTrigger("0 0 12 * * *", RunOnStartup = false)]TimerInfo myTimer)
        {
            await _ioTEdgeLayeredDeploymentBuilder.ApplyDeployments();
        }
    }
}
