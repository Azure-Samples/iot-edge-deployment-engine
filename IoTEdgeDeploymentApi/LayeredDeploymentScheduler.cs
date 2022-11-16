using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace IoTEdgeDeploymentApi
{
    public class LayeredDeploymentScheduler
    {
        [FunctionName("LayeredDeploymentScheduler")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer)
        {
        }
    }
}
