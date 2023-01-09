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
using Azure.Security.KeyVault.Secrets;
using IoTEdgeDeploymentEngine.Extension;
using Polly;
using Polly.Registry;

DotEnv.Load();
var host = ConfigureServices(args);
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Host created.");

var serviceDeployment = host.Services.GetRequiredService<IIoTEdgeDeploymentBuilder>();
await serviceDeployment.ApplyDeployments();

return;

IHost ConfigureServices(string[] args)
{
	var iotHubHostName = EnvReader.GetStringValue("IOTHUB_HOSTNAME");
	var keyVaultUri = EnvReader.GetStringValue("KEYVAULT_URI");
    var rootDirectory = EnvReader.GetStringValue("ROOT_MANIFESTS_FOLDER");
    string rootDirectoryAutomatic = Path.Combine(rootDirectory, "AutomaticDeployment");
    string rootDirectoryLayered = Path.Combine(rootDirectory, "LayeredDeployment");

	TokenCredential tokenCredential = new DefaultAzureCredential();
	return Host.CreateDefaultBuilder(args)
		.ConfigureServices((_, services) =>
			services
				.AddSingleton<RegistryManager>((s) => RegistryManager.Create(iotHubHostName, tokenCredential))				
				.AddSingleton<SecretClient>((s) => new SecretClient(new Uri(keyVaultUri), tokenCredential, new SecretClientOptions()
				{
					Retry = 
					{
						MaxRetries = 3,
						Delay = TimeSpan.FromSeconds(5),
						MaxDelay = TimeSpan.FromSeconds(15),
						Mode = RetryMode.Exponential,
						NetworkTimeout = TimeSpan.FromSeconds(60)
					}
				}))
				.AddSingleton<IKeyVaultAccessor, KeyVaultAccessor>()
				.AddScoped<IIoTEdgeDeploymentBuilder, IoTEdgeDeploymentBuilder>()
				.AddScoped<IPolicyRegistry<string>>(_ =>
				{
					var policyRegistry = new PolicyRegistry();
					policyRegistry
						.AddExponentialBackoffRetryPolicy()
						.AddInfiniteRetryPolicy()
						.AddCircuitBreakerPolicy();
					return policyRegistry;
				})
				.AddScoped<IIoTEdgeDeploymentBuilder, IoTEdgeDeploymentBuilder>()
				.AddSingleton<IIoTHubAccessor, IoTHubAccessor>()
				.AddSingleton<IManifestConfig>(c => new ManifestConfig
				{
					DirectoryRootAutomatic = rootDirectoryAutomatic,
					DirectoryRootLayered = rootDirectoryLayered
				})
				.AddLogging())
		.Build();
}
