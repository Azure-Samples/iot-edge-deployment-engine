using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Util;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentEngine
{
    /// <summary>
    /// Base class for the deployment engine classes providing common functionalities
    /// </summary>
    public class IoTEdgeDeploymentBuilderBase
    {
        private const string RouteKey = "route";

        /// <summary>
        /// Adds a deployment to the file system.
        /// </summary>
        /// <param name="filePath">Deployment full file path</param>
        /// <param name="fileContent">Deployment manifest JSON string</param>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public virtual async Task AddDeployment(string filePath, string fileContent)
        {
            if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(fileContent))
                await File.WriteAllTextAsync(filePath, fileContent);
            else
                throw new ArgumentNullException("FileName or FileContent");
        }
        
        /// <summary>
        /// Gets content of a single file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Dynamic object</returns>
        /// <exception cref="FileNotFoundException">FileNotFoundException</exception>
        public async Task<dynamic> GetFileContent(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filePath} not found.");
            
            var content = await File.ReadAllTextAsync(filePath);

            return JsonConvert.DeserializeObject<dynamic>(content);
        }
        
        /// <summary>
        /// Reads all routes from edgeHub modules and returns them in a list.
        /// </summary>
        /// <param name="deviceGroup">Set of devices grouped by tags</param>
        /// <returns></returns>
        protected List<Route> GetEdgeHubRoutes(IGrouping<string, Configuration> deviceGroup)
        {
            var flattenedRoutes = new List<Route>();
            foreach (var device in deviceGroup)
            {
                var desiredPropsString = device.Content.ModulesContent["$edgeHub"]["properties.desired"].ToString();
                var desiredProps =
                    JsonConvert.DeserializeObject<DeploymentManifest.PropertiesDesiredEdgeHub>(desiredPropsString);

                if (desiredProps?.Routes == null) continue;
                foreach (var route in desiredProps?.Routes)
                {
                    if (!flattenedRoutes.Any(f => f.Name == route.Key))
                        flattenedRoutes.Add(new Route(route.Key, route.Value[RouteKey]));
                }
            }

            return flattenedRoutes;
        }

        /// <summary>
        /// Reads edgeAgent module settings and return RegistryCredentials
        /// </summary>
        /// <param name="deviceGroup">Set of devices grouped by tags</param>
        /// <returns></returns>
        protected IEnumerable<RegistryCredential> GetEdgeAgentSettings(IGrouping<string, Configuration> deviceGroup)
        {
            var edgeAgentModules = deviceGroup.SelectMany(c => c.Content.ModulesContent["$edgeAgent"])
                .Distinct(new KeyValuePairEqualComparer());

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
        /// Reads edgeAgents module specification
        /// </summary>
        /// <param name="deviceGroup">Set of devices grouped by tags</param>
        /// <returns></returns>
        protected List<EdgeModuleSpecification> GetEdgeAgentModulesSpec(IGrouping<string, Configuration> deviceGroup)
        {
            var edgeAgentModules = deviceGroup.SelectMany(c => c.Content.ModulesContent["$edgeAgent"])
                .Distinct(new KeyValuePairEqualComparer());

            var modulesSpec = new List<EdgeModuleSpecification>();
            foreach (var edgeAgentModule in edgeAgentModules)
            {
                if (edgeAgentModule.Key != "properties.desired")
                {
                    var edgeAgentJson = edgeAgentModule.Value.ToString();
                    var mod = JsonConvert
                        .DeserializeObject<DeploymentManifest.SystemModuleSpecification>(edgeAgentJson);
                    var envVars = mod?.Env?.Select(e => new EnvironmentVariable(e.Key, e.Value.Value)).ToList();

                    var moduleName = edgeAgentModule.Key.Replace("properties.desired.modules.", "");
                    modulesSpec.Add(new EdgeModuleSpecification(moduleName, mod?.Settings?.Image, "1.0",
                        Enum.Parse<RestartPolicy>(mod?.RestartPolicy, true), mod?.Settings?.CreateOptions,
                        Enum.Parse<ModuleStatus>(mod?.Status, true), envVars));
                }
                else
                {
                    dynamic edgeAgentValue = edgeAgentModule.Value;
                    foreach (var moduleSpecification in edgeAgentValue["modules"])
                    {
                        DeploymentManifest.SystemModuleSpecification mod = JsonConvert
                            .DeserializeObject<DeploymentManifest.SystemModuleSpecification>(moduleSpecification.Value
                                .ToString());
                        var envVars = mod?.Env?.Select(e => new EnvironmentVariable(e.Key, e.Value.Value)).ToList();
                        modulesSpec.Add(new EdgeModuleSpecification(moduleSpecification.Name?.ToString(),
                            mod?.Settings?.Image, "1.0",
                            Enum.Parse<RestartPolicy>(mod?.RestartPolicy, true), mod?.Settings?.CreateOptions,
                            Enum.Parse<ModuleStatus>(mod?.Status, true), envVars));
                    }
                }
            }

            return modulesSpec;
        }

        /// <summary>
        /// Reads all files and returns their contents in a list of configuration objects
        /// </summary>
        /// <param name="directory">Directory of deployment files</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">FileNotFoundException</exception>
        protected async Task<IEnumerable<Configuration>> ReadAllFiles(string directory)
        {
            //_logger.LogTrace("Reading layered deployment files");
            var fileContents = new List<Configuration>();
            foreach (string file in Directory.EnumerateFiles(directory, "*.json"))
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException($"File {file} not found.");

                var fileContent = await File.ReadAllTextAsync(file);
                var config = JsonConvert.DeserializeObject<Configuration>(fileContent);

                fileContents.Add(config);
            }

            //_logger.LogTrace("Layered deployment files successfully read");
            return fileContents;
        }
    }
}