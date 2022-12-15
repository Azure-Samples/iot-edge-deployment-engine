using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Accessor;
using IoTEdgeDeploymentEngine.Config;
using IoTEdgeDeploymentEngine.Extension;
using IoTEdgeDeploymentEngine.Util;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentEngine
{
	/// <summary>
	/// Implementation for the deployment engine providing common functionalities
	/// </summary>
	public class IoTEdgeDeploymentBuilder : IIoTEdgeDeploymentBuilder
	{
		private readonly IIoTHubAccessor _ioTHubAccessor;
		private readonly IManifestConfig _manifestConfig;

		// / <summary>
		// / Returns path for deployment manifest files
		// / </summary>
		// protected abstract string ManifestDirectory { get; }

		/// <summary>
		/// ctor
		/// </summary>
		public IoTEdgeDeploymentBuilder(IIoTHubAccessor ioTHubAccessor, IManifestConfig manifestConfig)
		{
			_ioTHubAccessor = ioTHubAccessor;
			_manifestConfig = manifestConfig;
			//_logger = logger;
		}

		/// <inheritdoc />
		public async Task ApplyDeployments()
		{
			var files = await ReadAllFiles(_manifestConfig.DirectoryRootAutomatic,
				_manifestConfig.DirectoryRootLayered);

			var assignments = await CreateDeviceDeploymentAssignments(files);

			//var deviceGroups = configurations.GroupBy(c => c.TargetCondition);
			var tasks = new List<Task>();
			foreach (var assignment in assignments)
			{
				tasks.Add(ProcessDeviceAssignment(assignment));
			}

			await Task.WhenAll(tasks);
		}

		private async Task ProcessDeviceAssignment(KeyValuePair<string, List<DeploymentConfig>> assignment)
		{
			if (!assignment.Value.Any(a => a.Category == DeploymentCategory.AutomaticDeployment))
			{
				//todo: log warning
				return;
			}

			//https://learn.microsoft.com/en-us/azure/iot-edge/module-deployment-monitoring?view=iotedge-1.4#layered-deployment
			//layered deployments must have higher priority than the automatic deployment with highest priority
			var lowestPrio = assignment.Value.Where(a => a.Category == DeploymentCategory.AutomaticDeployment)
				.Max(a => a.Priority);
			var ordered = assignment.Value.Where(a => a.Priority >= lowestPrio).OrderBy(a => a.Priority).ToArray();

			var edgeAgentModules = ordered.GetEdgeAgentModules().ToArray();
			var edgeHubProps = ordered.GetEdgeHubProps().ToArray();
			var edgeAgentDesiredProperties = new EdgeAgentDesiredProperties()
			{
				SystemModuleVersion = edgeAgentModules.GetEdgeAgentSchemaVersion(),
				RegistryCredentials = edgeAgentModules.GetEdgeAgentRegCreds().ToList(),
				EdgeModuleSpecifications = edgeAgentModules.GetEdgeAgentModulesSpec(),
				EdgeSystemModuleSpecifications = edgeAgentModules.GetEdgeAgentSystemModulesSpec()
			};

			EdgeHubDesiredProperties edgeHubConfig = new EdgeHubDesiredProperties()
			{
				Routes = edgeHubProps.GetEdgeHubRoutes()
			};

			var configurationContent = new ConfigurationContent()
				.SetEdgeHub(edgeHubConfig)
				.SetEdgeAgent(edgeAgentDesiredProperties);

			var outerModules = ordered.GetOuterModules();
			foreach (var outerModule in outerModules)
			{
				if (!outerModule.Value.ContainsKey("properties.desired"))
					continue;

				configurationContent.SetModuleDesiredProperty(new ModuleSpecificationDesiredProperties()
				{
					Name = outerModule.Key,
					DesiredProperties = outerModule.Value["properties.desired"],
				});
			}

			await _ioTHubAccessor.ApplyDeploymentPerDevice(assignment.Key, configurationContent);
		}

		private async Task<Dictionary<string, List<DeploymentConfig>>> CreateDeviceDeploymentAssignments(Dictionary<string, Configuration> files)
		{
			var assignments = new Dictionary<string, List<DeploymentConfig>>();
			foreach (var config in files)
			{
				var deviceIds = await _ioTHubAccessor.GetDeviceIdsByCondition(config.Value.TargetCondition);
				foreach (var deviceId in deviceIds)
				{
					if (!assignments.ContainsKey(deviceId))
						assignments.Add(deviceId, new List<DeploymentConfig>());

					assignments[deviceId].Add(new DeploymentConfig()
					{
						ManifestFile = config.Key,
						Priority = config.Value.Priority,
						Category = config.Key.Contains(_manifestConfig.DirectoryRootAutomatic)
							? DeploymentCategory.AutomaticDeployment
							: DeploymentCategory.LayeredDeployment,
						ManifestConfig = config.Value
					});
				}
			}

			return assignments;
		}

		/// <summary>
		/// Adds a deployment to the file system.
		/// </summary>
		/// <param name="fileName">Deployment file name</param>
		/// <param name="fileContent">Deployment manifest JSON string</param>
		/// <param name="category">Deployment category (automatic, layered)</param>
		/// <exception cref="ArgumentNullException">ArgumentNullException</exception>
		public async Task AddDeployment(string fileName, string fileContent, DeploymentCategory category)
		{
			if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileContent))
			{
				var directory = category == DeploymentCategory.AutomaticDeployment
					? _manifestConfig.DirectoryRootAutomatic
					: _manifestConfig.DirectoryRootLayered;
				var fileLocation = Path.Combine(directory, fileName);
				await File.WriteAllTextAsync(fileLocation, fileContent);
			}
			else
				throw new ArgumentNullException("FileName or FileContent");
		}

		/// <summary>
		/// Gets content of a single file.
		/// </summary>
		/// <param name="fileName">Relative file path, name of the file with extension.</param>
		/// <param name="category">Deployment category (automatic, layered)</param>
		/// <returns>Dynamic object</returns>
		/// <exception cref="FileNotFoundException">FileNotFoundException</exception>
		public async Task<dynamic> GetFileContent(string fileName, DeploymentCategory category)
		{
			var directory = category == DeploymentCategory.AutomaticDeployment
				? _manifestConfig.DirectoryRootAutomatic
				: _manifestConfig.DirectoryRootLayered;
			var fileLocation = Path.Combine(directory, fileName);
			if (!File.Exists(fileLocation))
				throw new FileNotFoundException($"File {fileLocation} not found.");

			var content = await File.ReadAllTextAsync(fileLocation);

			return JsonConvert.DeserializeObject<dynamic>(content);
		}
		
		/// <summary>
		/// Reads all files and returns their contents in a list of configuration objects
		/// </summary>
		/// <param name="directories">Directories of deployment files</param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException">FileNotFoundException</exception>
		private async Task<Dictionary<string, Configuration>> ReadAllFiles(params string[] directories)
		{
			//_logger.LogTrace("Reading layered deployment files");
			var fileContents = new Dictionary<string, Configuration>();
			foreach (var directory in directories)
			{
				foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
				{
					if (!File.Exists(file))
						throw new FileNotFoundException($"File {file} not found.");

					var fileContent = await File.ReadAllTextAsync(file);
					var config = JsonConvert.DeserializeObject<Configuration>(fileContent);

					fileContents.Add(file, config);
				}
			}

			//_logger.LogTrace("Layered deployment files successfully read");
			return fileContents;
		}
	}
}