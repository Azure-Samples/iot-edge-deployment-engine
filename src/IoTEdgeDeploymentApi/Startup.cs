using System;
using IoTEdgeDeploymentApi;
using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.Accessor;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenApiHttpTriggerAuthorization = IoTEdgeDeploymentApi.Security.OpenApiHttpTriggerAuthorization;

[assembly: FunctionsStartup(typeof(Startup))]

namespace IoTEdgeDeploymentApi
{
	/// <inheritdoc />
	public class Startup : FunctionsStartup
	{
		/// <inheritdoc />
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services
				.AddSingleton<RegistryManager>((s) =>
					RegistryManager.CreateFromConnectionString(
						Environment.GetEnvironmentVariable("IoTHubConnectionString")))
				.AddScoped<IoTEdgeLayeredDeploymentBuilder, IoTEdgeLayeredDeploymentBuilder>()
				.AddScoped<IoTEdgeAutomaticDeploymentBuilder, IoTEdgeAutomaticDeploymentBuilder>()
				.AddSingleton<IIoTHubAccessor, IoTHubAccessor>()
				.AddHttpContextAccessor()
				.AddSingleton<IOpenApiHttpTriggerAuthorization>(p =>
				{
					var accessor = p.GetService<IHttpContextAccessor>();
					var auth = new OpenApiHttpTriggerAuthorization() { HttpContextAccessor = accessor };

					return auth;
				})
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
						IncludeRequestingHostName =
							DefaultOpenApiConfigurationOptions.IsFunctionsRuntimeEnvironmentDevelopment(),
						ForceHttps = DefaultOpenApiConfigurationOptions.IsHttpsForced(),
						ForceHttp = DefaultOpenApiConfigurationOptions.IsHttpForced(),
					};

					return options;
				});
		}
	}
}