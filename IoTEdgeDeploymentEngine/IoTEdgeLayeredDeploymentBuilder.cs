using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Tools;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

//using Microsoft.Extensions.Logging;


namespace IoTEdgeDeploymentEngine
{
	/// <inheritdoc/>
	public class IoTEdgeLayeredDeploymentBuilder : IIoTEdgeDeploymentBuilder
	{
		private const string MANIFEST_DIRECTORY = "./DeploymentFiles/LayeredDeployment";
		private const string ROUTE_KEY = "route";

		public IoTEdgeLayeredDeploymentBuilder()
		{
			//_logger = logger;
		}
		public async Task ApplyDeployments()
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
					var desiredProps = JsonConvert.DeserializeObject<DeploymentManifest.PropertiesDesiredEdgeHub>(desiredPropsString);

					foreach (var route in desiredProps.Routes)
					{
						if (!flattenedRoutes.Any(f => f.Name == route.Key))
							flattenedRoutes.Add(new Route(route.Key, route.Value[ROUTE_KEY]));
					}
				}
			}
		}

		public async Task AddDeployment(string filePath, string fileContent)
		{
			if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(fileContent))
				await File.WriteAllTextAsync(filePath, fileContent);
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