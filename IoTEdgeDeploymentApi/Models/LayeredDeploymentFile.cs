namespace IoTEdgeDeploymentApi.Models
{
	/// <summary>
	/// Specifies a file name and content for the deployment manifest.
	/// </summary>
	public class LayeredDeploymentFile
	{
		/// <summary>
		/// Full path and name of the deployment manifest file.
		/// </summary>
		public string FullFileName { get; set; }

		/// <summary>
		/// File Content of the deployment manifest in JSON string format.
		/// </summary>
		public string FileContent { get; set; }
	}
}

