using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.BusinessLogic;
using Microsoft.Azure.Devices;
//using Microsoft.Extensions.Logging;


namespace IoTEdgeDeploymentEngine
{
	/// <inheritdoc/>
	public class IoTEdgeDeploymentBuilder : IIoTEdgeDeploymentBuilder
	{
		private readonly ILayeredDeploymentLogic _layeredDeploymentLogic;
		//private readonly ILogger<RegistryManager> _logger;

		//public IoTEdgeDeploymentBuilder(ILogger<RegistryManager> logger, ILayeredDeploymentLogic layeredDeploymentLogic)
		public IoTEdgeDeploymentBuilder(ILayeredDeploymentLogic layeredDeploymentLogic)
		{
			_layeredDeploymentLogic = layeredDeploymentLogic;
			//_logger = logger;
		}

		public async Task ApplyAutomaticDeployments()
		{
			//_logger.LogInformation("Entered");

			return;
		}

		public async Task ApplyLayeredDeployments()
		{
			//_logger.LogInformation("Entered");
			await _layeredDeploymentLogic.GetDevicesFromManifests();
		}

		public async Task AddLayeredDeployment(string fileName, string fileContent)
		{
			await _layeredDeploymentLogic.AddLayeredDeployment(fileName, fileContent);
		}
	}
}