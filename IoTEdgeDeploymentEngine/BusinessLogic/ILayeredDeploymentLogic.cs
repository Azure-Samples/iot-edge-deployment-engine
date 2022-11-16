using System.Threading.Tasks;

namespace IoTEdgeDeploymentEngine.BusinessLogic
{
	/// <summary>
	/// Business logic for layered deployment
	/// </summary>
	public interface ILayeredDeploymentLogic
	{
		/// <summary>
		/// Extracts devices from deployment files
		/// </summary>
		/// <returns></returns>
		Task GetDevicesFromManifests();

		/// <summary>
		/// Adds layered deployments to file system.
		/// </summary>
		/// <param name="fileName">Layered Deployment file name</param>
		/// <param name="fileContent">Layered Deployment manifest JSON string</param>
		/// <returns></returns>
		Task AddLayeredDeployment(string fileName, string fileContent);
	}
}