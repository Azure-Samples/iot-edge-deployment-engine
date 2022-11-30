using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace IoTEdgeDeploymentEngine.Accessor
{
    /// <inheritdoc />
    public class IoTHubAccessor : IIoTHubAccessor
    {
        private readonly RegistryManager _registryManager;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="registryManager">RegistryManager instance</param>
        public IoTHubAccessor(RegistryManager registryManager)
        {
            _registryManager = registryManager;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetDeviceIdsByCondition(string targetCondition)
        {
            var sql = $"Select * from devices where {targetCondition}";
            var query = _registryManager.CreateQuery(sql);

            var deviceIds = new List<string>();
            while (query.HasMoreResults)
            {
                var twin = await query.GetNextAsTwinAsync();
                deviceIds.AddRange(twin.Select(f => f.DeviceId));
            }

            return deviceIds;
        }

        /// <inheritdoc />
        public async Task ApplyDeploymentPerDevice(string deviceId, ConfigurationContent configurationContent)
        {
            await _registryManager.ApplyConfigurationContentOnDeviceAsync(deviceId, configurationContent);
        }
    }
}