using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.Accessor;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using dotenv.net;
using IoTEdgeDeploymentEngine.Config;
using dotenv.net.Utilities;
using Azure.Identity;
using Azure.Core;

DotEnv.Load();
var host = ConfigureServices(args);
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Host created.");

var serviceLayered = host.Services.GetRequiredService<IoTEdgeLayeredDeploymentBuilder>();
await serviceLayered.ApplyDeployments();
var serviceAutomatic = host.Services.GetRequiredService<IoTEdgeAutomaticDeploymentBuilder>();
await serviceAutomatic.ApplyDeployments();

return;

IHost ConfigureServices(string[] args)
{
    var iotHubHostName = EnvReader.GetStringValue("IOTHUB_HOSTNAME");
    var rootDirectory = EnvReader.GetStringValue("ROOT_MANIFESTS_FOLDER");
    string rootDirectoryAutomatic = Path.Combine(rootDirectory, "AutomaticDeployment");
    string rootDirectoryLayered = Path.Combine(rootDirectory, "LayeredDeployment");

    return Host.CreateDefaultBuilder(args)
		.ConfigureServices((_, services) =>
			services.AddScoped<RegistryManager>((s) => {
				TokenCredential tokenCredential = new DefaultAzureCredential();
				return RegistryManager.Create(iotHubHostName, tokenCredential);
				})
                .AddScoped<IoTEdgeLayeredDeploymentBuilder, IoTEdgeLayeredDeploymentBuilder>()
				.AddScoped<IoTEdgeAutomaticDeploymentBuilder, IoTEdgeAutomaticDeploymentBuilder>()
				.AddSingleton<IIoTHubAccessor, IoTHubAccessor>()
				.AddSingleton<ManifestConfigAutomatic>(c => new ManifestConfigAutomatic { DirectoryRoot = rootDirectoryAutomatic })
                .AddSingleton<ManifestConfigLayered>(c => new ManifestConfigLayered { DirectoryRoot = rootDirectoryLayered })
                .AddLogging())
		.Build();
}
