using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.BusinessLogic;
using Microsoft.Azure.Devices;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services
				.AddSingleton<RegistryManager>((s) => RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubConnectionString")))
				.AddSingleton<ILayeredDeploymentLogic, LayeredDeploymentLogic>()
				.AddSingleton<IIoTEdgeDeploymentBuilder, IoTEdgeDeploymentBuilder>()
				.AddSingleton<IOpenApiConfigurationOptions>(_ =>
							{
								var options = new OpenApiConfigurationOptions()
								{
									Info = new OpenApiInfo()
									{
										Version = DefaultOpenApiConfigurationOptions.GetOpenApiDocVersion(),
										Title = DefaultOpenApiConfigurationOptions.GetOpenApiDocTitle(),
										Description = DefaultOpenApiConfigurationOptions.GetOpenApiDocDescription(),
										TermsOfService = new Uri("https://github.com/Azure/azure-functions-openapi-extension"),
										License = new OpenApiLicense()
										{
											Name = "MIT",
											Url = new Uri("http://opensource.org/licenses/MIT"),
										}
									},
									Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
									OpenApiVersion = DefaultOpenApiConfigurationOptions.GetOpenApiVersion(),
									IncludeRequestingHostName = DefaultOpenApiConfigurationOptions.IsFunctionsRuntimeEnvironmentDevelopment(),
									ForceHttps = DefaultOpenApiConfigurationOptions.IsHttpsForced(),
									ForceHttp = DefaultOpenApiConfigurationOptions.IsHttpForced(),
								};

								return options;
							});
		}
	}
}