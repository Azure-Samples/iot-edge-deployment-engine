using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;

namespace IoTEdgeDeploymentEngine.Accessor
{
	/// <inheritdoc />
	public class KeyVaultAccessor : IKeyVaultAccessor
	{
		private readonly SecretClient _secretClient;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="secretClient"></param>
		public KeyVaultAccessor(SecretClient secretClient)
		{
			_secretClient = secretClient;
		}

		/// <inheritdoc />
		public async Task<string> GetSecretByName(string secretName)
		{
			var secret = await _secretClient.GetSecretAsync(secretName);
			
			return secret?.Value.Value;
		}
	}
}