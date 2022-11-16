using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Tools;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
//using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Microsoft.Azure.Devices.DeploymentManifest;

namespace IoTEdgeDeploymentEngine.BusinessLogic
{
	/// <inheritdoc />
	public class LayeredDeploymentLogic : ILayeredDeploymentLogic
	{
		private const string MANIFEST_DIRECTORY = "./DeploymentFiles/LayeredDeployment";
		private const string ROUTE_KEY = "route";
		//private readonly ILogger<LayeredDeploymentLogic> _logger;

		//public LayeredDeploymentLogic(ILogger<LayeredDeploymentLogic> logger)
		public LayeredDeploymentLogic()
		{
			//_logger = logger;
		}

		public async Task GetDevicesFromManifests()
		{
			var configurations = await GetFileContent();
			
			var deviceGroups = configurations.GroupBy(c => c.TargetCondition);
			foreach (var deviceGroup in deviceGroups)
			{
				var modules = deviceGroup.SelectMany(c => c.Content.ModulesContent["$edgeAgent"])
					.Distinct(new KeyValuePairEqualComparer());
				var mods = deviceGroup.SelectMany(c =>
						c.Content.ModulesContent.Where(m => m.Key != "$edgeAgent" && m.Key != "$edgeHub"))
					.Distinct(new KeyValuePairDictionaryEqualComparer());
				
				var flattenedRoutes = new List<Route>();
				foreach (var device in deviceGroup)
				{
					var desiredPropsString = device.Content.ModulesContent["$edgeHub"]["properties.desired"].ToString();
					var desiredProps = JsonConvert.DeserializeObject<PropertiesDesiredEdgeHub>(desiredPropsString);

					foreach (var route in desiredProps.Routes)
					{
						if (!flattenedRoutes.Any(f => f.Name == route.Key))
							flattenedRoutes.Add(new Route(route.Key, route.Value[ROUTE_KEY]));
					}
				}
			}
		}

		public async Task AddLayeredDeployment(string fileName, string fileContent)
		{
			if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileContent))
				await File.WriteAllTextAsync(Path.Combine(MANIFEST_DIRECTORY, $"{fileName}.json"), fileContent);
			else
				throw new ArgumentNullException("FileName or FileContent");
		}

		private async Task<IEnumerable<Configuration>> GetFileContent()
		{
			//_logger.LogTrace("Reading layered deployment files");
			var fileContents = new List<Configuration>();
			foreach (string file in Directory.EnumerateFiles(MANIFEST_DIRECTORY, "*.json"))
			{
				string content = await File.ReadAllTextAsync(file);
				var config = JsonConvert.DeserializeObject<Configuration>(content);

				fileContents.Add(config);
			}

			//_logger.LogTrace("Layered deployment files successfully read");
			return fileContents;
		}
	}
}

