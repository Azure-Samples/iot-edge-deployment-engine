using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using IoTEdgeDeploymentApi.Models;
using IoTEdgeDeploymentEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentApi
{

	public class LayeredDeployment
	{
		private readonly IIoTEdgeDeploymentBuilder _ioTEdgeDeploymentBuilder;

		public LayeredDeployment(IIoTEdgeDeploymentBuilder ioTEdgeDeploymentBuilder)
		{
			_ioTEdgeDeploymentBuilder = ioTEdgeDeploymentBuilder;
		}

		[FunctionName("IoTEdgeLayeredDeployment")]
		[OpenApiOperation(operationId: "AddLayeredDeployment", tags: new[] { "name" })]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LayeredDeploymentFile), Required = true, Description = "Specifies a file name without file extension and content for the deployment manifest")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> AddLayeredDeployment(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
		{
			try
			{
				string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
				var data = JsonConvert.DeserializeObject<LayeredDeploymentFile>(requestBody);

				await _ioTEdgeDeploymentBuilder.AddLayeredDeployment(data.FileName, data.FileContent);

				return new OkObjectResult(data.ToString());
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}
	}
}

