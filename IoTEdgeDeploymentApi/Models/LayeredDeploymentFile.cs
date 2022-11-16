namespace IoTEdgeDeploymentApi.Models
{
	/// <summary>
	/// Specifies a file name and content for the deployment manifest.
	/// </summary>
	public class LayeredDeploymentFile
	{
		/// <summary>
		/// File name of the deployment manifest without file extension.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// File Content of the deployment manifest in JSON string format.
		/// </summary>
		public string FileContent { get; set; }
	}
}

