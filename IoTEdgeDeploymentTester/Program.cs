using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.BusinessLogic;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = ConfigureServices(args);
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Host created.");

var service = host.Services.GetService<IIoTEdgeDeploymentBuilder>();
await service.ApplyLayeredDeployments();

return;

IHost ConfigureServices(string[] args)
{
	return Host.CreateDefaultBuilder(args)
				.ConfigureServices((_, services) =>
					services.AddSingleton<RegistryManager>((s) => RegistryManager.CreateFromConnectionString(args[0]))
							.AddSingleton<ILayeredDeploymentLogic, LayeredDeploymentLogic>()
							.AddSingleton<IIoTEdgeDeploymentBuilder, IoTEdgeDeploymentBuilder>()
							.AddLogging())
				.Build();
}
