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
    public class AutomaticDeploymentScheduler
    {
        private readonly IIoTEdgeDeploymentBuilder _ioTEdgeAutomaticDeploymentBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ioTEdgeAutomaticDeploymentBuilder">IoTEdgeAutomaticDeploymentBuilder instance per DI</param>
        public AutomaticDeploymentScheduler(IoTEdgeAutomaticDeploymentBuilder ioTEdgeAutomaticDeploymentBuilder)
        {
            _ioTEdgeAutomaticDeploymentBuilder = ioTEdgeAutomaticDeploymentBuilder;
        }
        
        /// <summary>
        /// Timer based function that executes deployments every day at 12:00am
        /// </summary>
        /// <param name="myTimer">Timer object</param>
        [FunctionName("AutomaticDeploymentScheduler")]
        public async Task Run([TimerTrigger("0 0 12 * * *", RunOnStartup = false)]TimerInfo myTimer)
        {
            await _ioTEdgeAutomaticDeploymentBuilder.ApplyDeployments();
        }
    }
}
