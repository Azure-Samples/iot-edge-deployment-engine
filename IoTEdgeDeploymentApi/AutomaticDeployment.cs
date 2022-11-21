using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using IoTEdgeDeploymentApi.Model;
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
	/// <summary>
	/// Http controller for layered deployment
	/// </summary>
	public class AutomaticDeployment
	{
		private readonly IIoTEdgeDeploymentBuilder _ioTEdgeAutomaticDeploymentBuilder;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="ioTEdgeAutomaticDeploymentBuilder">IoTEdgeAutomaticDeploymentBuilder instance per DI</param>
		public AutomaticDeployment(IoTEdgeAutomaticDeploymentBuilder ioTEdgeAutomaticDeploymentBuilder)
		{
			_ioTEdgeAutomaticDeploymentBuilder = ioTEdgeAutomaticDeploymentBuilder;
		}

		/// <summary>
		/// Creates a new automatic deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("AddAutomaticDeployment")]
		[OpenApiOperation(operationId: "AddAutomaticDeployment", tags: new[] { "IoTEdgeAutomaticDeployment" })]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DeploymentFile), Required = true, Description = "Specifies a file name without file extension and content for the deployment manifest")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> AddAutomaticDeployment(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
		{
			try
			{
				var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
				var data = JsonConvert.DeserializeObject<DeploymentFile>(requestBody);

				await _ioTEdgeAutomaticDeploymentBuilder.AddDeployment(data?.FullFileName, data?.FileContent);

				return new OkObjectResult("Succeeded");
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}
		
		/// <summary>
		/// Gets the content of an automatic deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("GetAutomaticDeploymentFileContent")]
		[OpenApiOperation(operationId: "GetAutomaticDeploymentFileContent", tags: new[] { "IoTEdgeAutomaticDeployment" })]
		[OpenApiParameter(name: "filePath", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **filePath** parameter")]		
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> GetAutomaticDeploymentFileContent(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
		{
			try
			{
				var filePath = req.Query["filePath"];
		
				var content = await _ioTEdgeAutomaticDeploymentBuilder.GetFileContent(filePath);
				
				return new OkObjectResult(content);
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}
	}
}

