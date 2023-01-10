using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTEdgeDeploymentEngine.Enums;
using Microsoft.Azure.Devices;
using Polly;
using Polly.Registry;

namespace IoTEdgeDeploymentEngine.Accessor
{
	/// <inheritdoc />
	public class IoTHubAccessor : IIoTHubAccessor
	{
		private readonly RegistryManager _registryManager;
		private readonly IAsyncPolicy _retryPolicy;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="registryManager">RegistryManager instance</param>
        /// <param name="policyRegistry">Retry policy</param>
        public IoTHubAccessor(RegistryManager registryManager, IPolicyRegistry<string> policyRegistry)
		{
			_registryManager = registryManager;
			
			_retryPolicy = policyRegistry.Get<IAsyncPolicy>(PolicyNames.ExponentialBackoffRetryPolicy.ToString());
			var circuitBreakerPolicy = policyRegistry.Get<IAsyncPolicy>(PolicyNames.CircuitBreakerPolicy.ToString());
			_retryPolicy.WrapAsync(circuitBreakerPolicy);
		}

		/// <inheritdoc />
		public async Task<IEnumerable<string>> GetDeviceIdsByCondition(string targetCondition)
		{
			var deviceIds = new List<string>();

			var sql = $"Select * from devices where {targetCondition}";

			await _retryPolicy.ExecuteAsync(async () =>
			{
				var query = _registryManager.CreateQuery(sql);

				while (query.HasMoreResults)
				{
					var twin = await query.GetNextAsTwinAsync();
					deviceIds.AddRange(twin.Select(f => f.DeviceId));
				}
			});
			return deviceIds;
		}

		/// <inheritdoc />
		public async Task ApplyDeploymentPerDevice(string deviceId, ConfigurationContent configurationContent)
		{
			await _retryPolicy.ExecuteAsync(async () =>
			{
				await _registryManager.ApplyConfigurationContentOnDeviceAsync(deviceId, configurationContent);
			});
		}
	}
}