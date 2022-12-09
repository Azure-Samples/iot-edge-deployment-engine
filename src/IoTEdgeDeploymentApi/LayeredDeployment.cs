using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using IoTEdgeDeploymentApi.Model;
using IoTEdgeDeploymentApi.Security;
using IoTEdgeDeploymentEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentApi
{
	/// <summary>
	/// Http controller for layered deployment
	/// </summary>
	public class LayeredDeployment
	{
		private readonly IIoTEdgeDeploymentBuilder _ioTEdgeLayeredDeploymentBuilder;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="ioTEdgeLayeredDeploymentBuilder">IoTEdgeLayeredDeploymentBuilder instance per DI</param>
		public LayeredDeployment(IoTEdgeLayeredDeploymentBuilder ioTEdgeLayeredDeploymentBuilder)
		{
			_ioTEdgeLayeredDeploymentBuilder = ioTEdgeLayeredDeploymentBuilder;
		}

		/// <summary>
		/// Creates a new layered deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("AddLayeredDeployment")]
		[OpenApiOperation(operationId: "AddLayeredDeployment", tags: new[] { "IoTEdgeLayeredDeployment" })]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DeploymentFile), Required = true, Description = "Specifies a file name with file extension and content for the deployment manifest")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		[OpenApiSecurity("implicit_auth", SecuritySchemeType.OAuth2, Flows = typeof(ImplicitAuthFlow))]
		[OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		public async Task<IActionResult> AddLayeredDeployment(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
		{
			try
			{
				var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
				var data = JsonConvert.DeserializeObject<DeploymentFile>(requestBody);

				await _ioTEdgeLayeredDeploymentBuilder.AddDeployment(data?.FileName, data?.FileContent);

				return new OkObjectResult("Succeeded");
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}
		
		/// <summary>
		/// Gets the content of a layered deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("GetLayeredDeploymentFileContent")]
		[OpenApiOperation(operationId: "GetLayeredDeploymentFileContent", tags: new[] { "IoTEdgeLayeredDeployment" })]
		[OpenApiParameter(name: "fileName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **fileName** parameter")]		
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		[OpenApiSecurity("implicit_auth", SecuritySchemeType.OAuth2, Flows = typeof(ImplicitAuthFlow))]
		[OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		public async Task<IActionResult> GetLayeredDeploymentFileContent(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
		{
			try
			{
				var fileName = req.Query["fileName"];
		
				var content = await _ioTEdgeLayeredDeploymentBuilder.GetFileContent(fileName);
				
				return new OkObjectResult(content);
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}

	}
}

