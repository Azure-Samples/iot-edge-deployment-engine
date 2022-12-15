using System;
using System.Collections.Generic;
using System.Linq;
using IoTEdgeDeploymentEngine.Config;
using IoTEdgeDeploymentEngine.Util;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentEngine.Extension
{
	/// <summary>
	/// Collection of extension methods to parse manifest file
	/// </summary>
	public static class ManifestParser
	{
		private const string RouteKey = "route";

		/// <summary>
		/// Gets the edgeAgents modules specification from ModulesContent
		/// </summary>
		/// <param name="deploymentConfigs">Set of deployments for single device</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, object>> GetEdgeAgentModules(
			this IEnumerable<DeploymentConfig> deploymentConfigs)
		{
			return deploymentConfigs.Where(c => c.ManifestConfig.Content.ModulesContent.ContainsKey("$edgeAgent"))
				.SelectMany(c => c.ManifestConfig.Content.ModulesContent["$edgeAgent"])
				.Distinct(new KeyValuePairEqualComparer());
		}

		/// <summary>
		/// Gets the edgeHub properties specification from ModulesContent
		/// </summary>
		/// <param name="deploymentConfigs">Set of deployments for single device</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, object>> GetEdgeHubProps(
			this IEnumerable<DeploymentConfig> deploymentConfigs)
		{
			return deploymentConfigs.Where(c => c.ManifestConfig.Content.ModulesContent.ContainsKey("$edgeHub"))
				.SelectMany(c => c.ManifestConfig.Content.ModulesContent["$edgeHub"])
				.Distinct(new KeyValuePairEqualComparer());
		}

		/// <summary>
		/// Gets the edgeAgents modules specification from ModulesContent
		/// </summary>
		/// <param name="deploymentConfigs">Set of deployments for single device</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, IDictionary<string, object>>> GetOuterModules(
			this IEnumerable<DeploymentConfig> deploymentConfigs)
		{
			return deploymentConfigs
				.SelectMany(c =>
					c.ManifestConfig.Content.ModulesContent.Where(m => m.Key != "$edgeAgent" && m.Key != "$edgeHub"))
				.Distinct(new KeyValuePairDictionaryEqualComparer());
		}
		
				/// <summary>
		/// Reads edgeAgents module specification
		/// </summary>
		/// <param name="edgeAgentModules">Modules content of edgeAgent specification</param>
		/// <returns></returns>
		public static List<EdgeModuleSpecification> GetEdgeAgentModulesSpec(
			this IEnumerable<KeyValuePair<string, object>> edgeAgentModules)
		{
			var modulesSpec = new List<EdgeModuleSpecification>();
			foreach (var edgeAgentModule in edgeAgentModules)
			{
				var moduleName = string.Empty;
				DeploymentManifest.SystemModuleSpecification mod = null;
				if (edgeAgentModule.Key != "properties.desired")
				{
					var edgeAgentJson = edgeAgentModule.Value.ToString();
					mod = JsonConvert
						.DeserializeObject<DeploymentManifest.SystemModuleSpecification>(edgeAgentJson);
					moduleName = edgeAgentModule.Key.Replace("properties.desired.modules.", "");
				}
				else
				{
					dynamic edgeAgentValue = edgeAgentModule.Value;
					foreach (var moduleSpecification in edgeAgentValue["modules"])
					{
						mod = JsonConvert
							.DeserializeObject<DeploymentManifest.SystemModuleSpecification>(moduleSpecification.Value
								.ToString());
						moduleName = moduleSpecification.Name?.ToString();
					}
				}

				if (null == mod) 
					continue;
				
				var envVars = mod?.Env?.Select(e => new EnvironmentVariable(e.Key, e.Value.Value)).ToList();
				modulesSpec.Add(new EdgeModuleSpecification(moduleName,
					mod?.Settings?.Image, "1.0",
					string.IsNullOrEmpty(mod?.RestartPolicy)
						? RestartPolicy.Always
						: Enum.Parse<RestartPolicy>(mod?.RestartPolicy, true),
					mod?.Settings?.CreateOptions == null ? string.Empty : JsonConvert.SerializeObject(mod.Settings.CreateOptions),
					string.IsNullOrEmpty(mod?.Status)
						? ModuleStatus.Running
						: Enum.Parse<ModuleStatus>(mod?.Status, true),
					envVars));
			}

			return modulesSpec;
		}

		/// <summary>
		/// Reads edgeAgents system module specification
		/// </summary>
		/// <param name="edgeAgentModules">Modules content of edgeAgent Specification</param>
		/// <returns></returns>
		public static List<EdgeModuleSpecification> GetEdgeAgentSystemModulesSpec(
			this IEnumerable<KeyValuePair<string, object>> edgeAgentModules)
		{
			var systemModulesSpec = new List<EdgeModuleSpecification>();
			foreach (var edgeAgentModule in edgeAgentModules)
			{
				if (edgeAgentModule.Key != "properties.desired")
					continue;

				dynamic edgeAgentValue = edgeAgentModule.Value;
				foreach (var moduleSpecification in edgeAgentValue["systemModules"])
				{
					DeploymentManifest.SystemModuleSpecification mod = JsonConvert
						.DeserializeObject<DeploymentManifest.SystemModuleSpecification>(moduleSpecification.Value
							.ToString());
					var envVars = mod?.Env?.Select(e => new EnvironmentVariable(e.Key, e.Value.Value)).ToList();
					systemModulesSpec.Add(new EdgeModuleSpecification(moduleSpecification.Name?.ToString(),
						mod?.Settings?.Image, "1.0",
						string.IsNullOrEmpty(mod?.RestartPolicy)
							? RestartPolicy.Always
							: Enum.Parse<RestartPolicy>(mod?.RestartPolicy, true),
						mod?.Settings?.CreateOptions == null ? string.Empty : JsonConvert.SerializeObject(mod.Settings.CreateOptions),
						string.IsNullOrEmpty(mod?.Status)
							? ModuleStatus.Running
							: Enum.Parse<ModuleStatus>(mod?.Status, true),
						envVars));
				}
			}

			return systemModulesSpec;
		}

		/// <summary>
		/// Reads all routes from edgeHub modules and returns them in a list.
		/// </summary>
		/// <param name="edgeHubProps">Content of edgeHub properties</param>
		/// <returns></returns>
		public static List<Route> GetEdgeHubRoutes(this IEnumerable<KeyValuePair<string, object>> edgeHubProps)
		{
			var flattenedRoutes = new List<Route>();
			foreach (var routeProp in edgeHubProps)
			{
				dynamic routePropValue = routeProp.Value;
				if (routeProp.Key != "properties.desired")
				{
					var key = routeProp.Key.Replace("properties.desired.routes.", "");
					foreach (var route in routePropValue)
					{
						if (!flattenedRoutes.Any(f => f.Name == key))
							flattenedRoutes.Add(new Route(key, route.Value?.ToString()));
					}
				}
				else
				{
					var desiredProps =
						JsonConvert.DeserializeObject<DeploymentManifest.PropertiesDesiredEdgeHub>(
							routePropValue.ToString());

					if (desiredProps?.Routes == null) continue;
					foreach (var route in desiredProps?.Routes)
					{
						if (!flattenedRoutes.Any(f => f.Name == route.Key))
							flattenedRoutes.Add(new Route(route.Key, route.Value[RouteKey]));
					}
				}
			}

			return flattenedRoutes;
		}
		
		/// <summary>
		/// Reads edgeAgent module settings and returns RegistryCredentials
		/// </summary>
		/// <param name="edgeAgentModules">Modules content of edgeAgent specification</param>
		/// <returns></returns>
		public static IEnumerable<RegistryCredential> GetEdgeAgentRegCreds(this IEnumerable<KeyValuePair<string, object>> edgeAgentModules)
		{
			foreach (var edgeAgentModule in edgeAgentModules)
			{
				if (edgeAgentModule.Key != "properties.desired") continue;

				dynamic edgeAgentValue = edgeAgentModule.Value;
				var edgeAgentRuntimeJson = edgeAgentValue["runtime"]?["settings"]?["registryCredentials"]?.ToString();
				if (string.IsNullOrEmpty(edgeAgentRuntimeJson))
					continue;

				Dictionary<string, DeploymentManifest.Store> runtime =
					JsonConvert.DeserializeObject<Dictionary<string, DeploymentManifest.Store>>(
						edgeAgentRuntimeJson);
				foreach (var regCred in runtime)
				{
					yield return new RegistryCredential(regCred.Key,
						regCred.Value?.Address, regCred.Value?.Username,
						regCred.Value?.Password);
				}
			}
		}

		/// <summary>
		/// Reads edgeAgent module settings and returns SchemaVersion
		/// </summary>
		/// <param name="edgeAgentModules">Modules content of edgeAgent specification</param>
		/// <returns></returns>
		public static string GetEdgeAgentSchemaVersion(this IEnumerable<KeyValuePair<string, object>> edgeAgentModules)
		{
			foreach (var edgeAgentModule in edgeAgentModules)
			{
				if (edgeAgentModule.Key != "properties.desired") continue;

				dynamic edgeAgentValue = edgeAgentModule.Value;
				var schemaVersion = edgeAgentValue["schemaVersion"]?.ToString();
				if (!string.IsNullOrEmpty(schemaVersion))
					return schemaVersion;
			}

			return string.Empty;
		}
	}
}