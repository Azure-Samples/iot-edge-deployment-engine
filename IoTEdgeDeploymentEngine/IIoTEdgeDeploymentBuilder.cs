using System.Threading.Tasks;

namespace IoTEdgeDeploymentEngine
{
	/// <summary>
	/// Interface as an entry point for the IoTEdgeDeploymentEngine.
	/// </summary>
	public interface IIoTEdgeDeploymentBuilder
	{
		/// <summary>
		/// Applies automatic deployments for a subset of defined devices per query.
		/// </summary>
		/// <returns></returns>
		Task ApplyAutomaticDeployments();

		/// <summary>
		/// Applies layered deployments for a subset of defined devices per query.
		/// </summary>
		/// <returns></returns>
		Task ApplyLayeredDeployments();

		/// <summary>
		/// Adds layered deployments to file system.
		/// </summary>
		/// <param name="fileName">Layered Deployment file name</param>
		/// <param name="fileContent">Layered Deployment manifest JSON string</param>
		/// <returns></returns>
		Task AddLayeredDeployment(string fileName, string fileContent);
	}
}