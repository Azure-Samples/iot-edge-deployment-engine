﻿using System;
using System.IO;
using IoTEdgeDeploymentApi;
using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.Accessor;
using IoTEdgeDeploymentEngine.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenApiHttpTriggerAuthorization = IoTEdgeDeploymentApi.Security.OpenApiHttpTriggerAuthorization;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

[assembly: FunctionsStartup(typeof(Startup))]

namespace IoTEdgeDeploymentApi
{
	/// <inheritdoc />
	public class Startup : FunctionsStartup
	{
		/// <inheritdoc />
		public override void Configure(IFunctionsHostBuilder builder)
		{
			var rootDirectory = Environment.GetEnvironmentVariable("ROOT_MANIFESTS_FOLDER");
			var iotHubHostname = Environment.GetEnvironmentVariable("IOTHUB_HOSTNAME");
			var keyVaultUri = Environment.GetEnvironmentVariable("KEYVAULT_URI");
			string rootDirectoryAutomatic = Path.Combine(rootDirectory, "AutomaticDeployment");
			string rootDirectoryLayered = Path.Combine(rootDirectory, "LayeredDeployment");
			CreateManifestSubFolders(rootDirectoryAutomatic);
			CreateManifestSubFolders(rootDirectoryLayered);

			TokenCredential token = new DefaultAzureCredential();

			builder.Services
				.AddSingleton<RegistryManager>((s) =>
					RegistryManager.Create(iotHubHostname, token))
				.AddSingleton<SecretClient>((s) => new SecretClient(new Uri(keyVaultUri), token))
				.AddScoped<IKeyVaultAccessor, KeyVaultAccessor>()
				.AddScoped<IIoTEdgeDeploymentBuilder, IoTEdgeDeploymentBuilder>()
				.AddSingleton<IIoTHubAccessor, IoTHubAccessor>()
				.AddSingleton<IManifestConfig>(c => new ManifestConfig
				{
					DirectoryRootAutomatic = rootDirectoryAutomatic,
					DirectoryRootLayered = rootDirectoryLayered
				})
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
				})
				.AddLogging();
		}

		private void CreateManifestSubFolders(string directory)
		{
			if (!Directory.Exists(directory))
			{ 
				Directory.CreateDirectory(directory); 
			}
        }
	}
}