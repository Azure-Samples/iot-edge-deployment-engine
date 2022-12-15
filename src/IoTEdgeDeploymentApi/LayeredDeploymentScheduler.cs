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
        private readonly IIoTEdgeDeploymentBuilder _ioTEdgeDeploymentBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ioTEdgeDeploymentBuilder">IoTEdgeDeploymentBuilder instance per DI</param>
        public LayeredDeploymentScheduler(IIoTEdgeDeploymentBuilder ioTEdgeDeploymentBuilder)
        {
            _ioTEdgeDeploymentBuilder = ioTEdgeDeploymentBuilder;
        }
        
        /// <summary>
        /// Timer based function that executes deployments every day at 12:00am
        /// </summary>
        /// <param name="myTimer">Timer object</param>
        [FunctionName("LayeredDeploymentScheduler")]
        public async Task Run([TimerTrigger("0 0 12 * * *", RunOnStartup = false)]TimerInfo myTimer)
        {
            await _ioTEdgeDeploymentBuilder.ApplyDeployments();
        }
    }
}
