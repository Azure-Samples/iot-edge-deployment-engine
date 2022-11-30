﻿using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.Accessor;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
	return Host.CreateDefaultBuilder(args)
		.ConfigureServices((_, services) =>
			services.AddSingleton<RegistryManager>((s) => RegistryManager.CreateFromConnectionString(args[0]))
				.AddScoped<IoTEdgeLayeredDeploymentBuilder, IoTEdgeLayeredDeploymentBuilder>()
				.AddScoped<IoTEdgeAutomaticDeploymentBuilder, IoTEdgeAutomaticDeploymentBuilder>()
				.AddSingleton<IIoTHubAccessor, IoTHubAccessor>()
				.AddLogging())
		.Build();
}